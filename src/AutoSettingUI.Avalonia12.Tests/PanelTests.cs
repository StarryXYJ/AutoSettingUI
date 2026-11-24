using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AutoSettingUI.Avalonia.Attributes;
using AutoSettingUI.Avalonia.Controls;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.DynamicLocalization;
using DynamicLocalization.Core;
using DynamicLocalization.Core.Providers;
using System.Globalization;
using NUnit.Framework;

[assembly: AvaloniaTestApplication(typeof(AutoSettingUI.Avalonia12.Tests.TestApp))]

namespace AutoSettingUI.Avalonia12.Tests;

public static class TestApp
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

[SettingUI]
public sealed class TestSettings
{
    [Title("Name")]
    public string Name { get; set; } = "Before";
    [Title("Samples"), NumericUpDown(Minimum = 1, Maximum = 128)]
    public int Samples { get; set; } = 16;
    [Title("Enabled"), CheckBox]
    public bool Enabled { get; set; } = true;
}

[SettingUI]
[MainHeader("settings.header", UseResourceKey = true)]
public sealed class LocalizedSettings
{
    [Title("settings.name", UseResourceKey = true)]
    [Placeholder("settings.placeholder", UseResourceKey = true)]
    public string Name { get; set; } = "";
}

public sealed class TestLocalizationService : ILocalizationService
{
    private EventHandler? _changed;
    public int SubscriberCount { get; private set; }
    public Dictionary<string, string> Translations { get; } = new();
    public string? GetString(string key) => Translations.GetValueOrDefault(key);
    public event EventHandler? LocalizationChanged
    {
        add { _changed += value; SubscriberCount++; }
        remove { _changed -= value; SubscriberCount--; }
    }
    public void Notify() => _changed?.Invoke(this, EventArgs.Empty);
}

public sealed class TestDynamicProvider : ILocalizationProvider
{
    public string Name => "Test";
    public IEnumerable<CultureInfo> GetAvailableCultures() => new[] { new CultureInfo("fr-FR"), new CultureInfo("en-US") };
    public string? GetString(string key, CultureInfo culture) => key == "language" ? culture.Name : null;
    public bool TryGetString(string key, CultureInfo culture, out string? value)
    {
        value = GetString(key, culture);
        return value != null;
    }
    public Task ReloadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class PanelTests
{
    [AvaloniaTest]
    public void CustomLocalizationUpdatesAndUnsubscribes()
    {
        var styles = Application.Current!.Styles;
        styles.Add(new FluentTheme());
        styles.Add(new StyleInclude(new Uri("avares://AutoSettingUI.Avalonia/"))
        {
            Source = new Uri("avares://AutoSettingUI.Avalonia/Themes/Generic.axaml")
        });
        var first = new TestLocalizationService();
        first.Translations["settings.header"] = "First header";
        first.Translations["settings.name"] = "First name";
        first.Translations["settings.placeholder"] = "First hint";
        var panel = new AvaloniaAutoSettingPanel
        {
            Targets = new object[] { new LocalizedSettings() },
            LocalizationService = first
        };
        var window = new Window { Width = 400, Height = 400, Content = panel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var textBox = panel.GetVisualDescendants().OfType<TextBox>().Single();
            var tree = panel.GetVisualDescendants().OfType<TreeView>().Single();
            var node = ((IEnumerable<NavigationNode>)tree.ItemsSource!).Single();
            Assert.That(node.Title, Is.EqualTo("First header"));
            Assert.That(textBox.PlaceholderText, Is.EqualTo("First hint"));
            Assert.That(panel.GetVisualDescendants().OfType<TextBlock>().Any(x => x.Text == "First name"), Is.True);

            first.Translations["settings.header"] = "Second header";
            first.Translations["settings.name"] = "Second name";
            first.Translations["settings.placeholder"] = "Second hint";
            Task.Run(first.Notify).GetAwaiter().GetResult();
            Dispatcher.UIThread.RunJobs();
            Assert.That(node.Title, Is.EqualTo("Second header"));
            Assert.That(textBox.PlaceholderText, Is.EqualTo("Second hint"));
            Assert.That(panel.GetVisualDescendants().OfType<TextBlock>().Any(x => x.Text == "Second name"), Is.True);

            var second = new TestLocalizationService();
            panel.LocalizationService = second;
            Assert.That(first.SubscriberCount, Is.Zero);
            Assert.That(second.SubscriberCount, Is.EqualTo(1));
            Assert.That(node.Title, Is.EqualTo("settings.header"));
            Assert.That(textBox.PlaceholderText, Is.EqualTo("settings.placeholder"));
            window.Content = null;
            Assert.That(second.SubscriberCount, Is.Zero);
            second.Translations["settings.header"] = "Reattached header";
            window.Content = panel;
            Dispatcher.UIThread.RunJobs();
            Assert.That(second.SubscriberCount, Is.EqualTo(1));
            Assert.That(node.Title, Is.EqualTo("Reattached header"));
            panel.LocalizationService = null;
            Assert.That(second.SubscriberCount, Is.Zero);
        }
        finally { window.Close(); }
    }

    [Test]
    public void DynamicLocalizationAdapterForwardsChangesAndCanDetach()
    {
        var service = new CultureService();
        var originalCulture = service.CurrentCulture;
        service.RegisterProvider(new TestDynamicProvider());
        var adapter = new DynamicLocalizationAdapter(service);
        var changes = 0;
        adapter.LocalizationChanged += (_, _) => changes++;
        try
        {
            Assert.That(adapter.GetString("missing"), Is.EqualTo(service.GetString("missing")));
            service.SetCulture("fr-FR");
            Assert.That(changes, Is.GreaterThan(0));
            Assert.That(adapter.GetString("language"), Is.EqualTo("fr-FR"));
            adapter.Dispose();
            var afterDispose = changes;
            service.SetCulture("en-US");
            Assert.That(changes, Is.EqualTo(afterDispose));
        }
        finally
        {
            adapter.Dispose();
            service.CurrentCulture = originalCulture;
        }
    }

    [AvaloniaTest]
    public void CompiledTemplateAndBindingsWorkWithAvalonia12()
    {
        var styles = Application.Current!.Styles;
        styles.Add(new FluentTheme());
        styles.Add(new StyleInclude(new Uri("avares://AutoSettingUI.Avalonia/"))
        {
            Source = new Uri("avares://AutoSettingUI.Avalonia/Themes/Generic.axaml")
        });
        var settings = new TestSettings();
        var panel = new AvaloniaAutoSettingPanel { Targets = new object[] { settings }, ShowNavigation = false };
        var window = new Window { Width = 400, Height = 400, Content = panel };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var controls = panel.GetVisualDescendants().ToArray();
            var text = controls.OfType<TextBox>().Single(x => x.Text == "Before");
            var number = controls.OfType<NumericUpDown>().Single();
            var toggle = controls.OfType<CheckBox>().Single();
            text.Text = "After";
            number.Value = 32;
            toggle.IsChecked = false;
            Dispatcher.UIThread.RunJobs();
            Assert.Multiple(() =>
            {
                Assert.That(settings.Name, Is.EqualTo("After"));
                Assert.That(settings.Samples, Is.EqualTo(32));
                Assert.That(settings.Enabled, Is.False);
            });
        }
        finally { window.Close(); }
    }
}
