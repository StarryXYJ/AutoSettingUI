using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AutoSettingUI.Avalonia.Controls;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.Ursa.Controls;
using NUnit.Framework;

[assembly: AvaloniaTestApplication(typeof(AutoSettingUI.Avalonia11.Tests.TestApp))]

namespace AutoSettingUI.Avalonia11.Tests;

public static class TestApp
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

[SettingUI]
[MainHeader("header", UseResourceKey = true)]
public sealed class Settings
{
    [Title("label", UseResourceKey = true)]
    public string Name { get; set; } = "";
}

public sealed class Localizer : ILocalizationService
{
    private EventHandler? _changed;
    public int Subscribers { get; private set; }
    public string Language { get; set; } = "one";
    public string? GetString(string key) => $"{Language}-{key}";
    public event EventHandler? LocalizationChanged
    {
        add { _changed += value; Subscribers++; }
        remove { _changed -= value; Subscribers--; }
    }
    public void Notify() => _changed?.Invoke(this, EventArgs.Empty);
}

public class LocalizationTests
{
    [AvaloniaTest]
    public void AvaloniaAndUrsaPanelsUpdateAndDetach()
    {
        var styles = Application.Current!.Styles;
        styles.Add(new FluentTheme());
        foreach (var assembly in new[] { "AutoSettingUI.Avalonia", "AutoSettingUI.Ursa" })
            styles.Add(new StyleInclude(new Uri($"avares://{assembly}/"))
            {
                Source = new Uri($"avares://{assembly}/Themes/Generic.axaml")
            });

        var service = new Localizer();
        var avalonia = new AvaloniaAutoSettingPanel { Targets = new object[] { new Settings() }, LocalizationService = service };
        var ursa = new UrsaAutoSettingPanel { Targets = new object[] { new Settings() }, LocalizationService = service };
        var host = new StackPanel { Children = { avalonia, ursa } };
        var window = new Window { Width = 800, Height = 800, Content = host };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.That(service.Subscribers, Is.EqualTo(2));
            CheckPanel(avalonia, "one-header", "one-label");
            CheckPanel(ursa, "one-header", "one-label");
            service.Language = "two";
            Task.Run(service.Notify).GetAwaiter().GetResult();
            Dispatcher.UIThread.RunJobs();
            CheckPanel(avalonia, "two-header", "two-label");
            CheckPanel(ursa, "two-header", "two-label");
            window.Content = null;
            Assert.That(service.Subscribers, Is.Zero);
            service.Language = "three";
            window.Content = host;
            Dispatcher.UIThread.RunJobs();
            Assert.That(service.Subscribers, Is.EqualTo(2));
            CheckPanel(avalonia, "three-header", "three-label");
            CheckPanel(ursa, "three-header", "three-label");
        }
        finally { window.Close(); }
    }

    private static void CheckPanel(TemplatedControl panel, string header, string label)
    {
        var descendants = panel.GetVisualDescendants().ToArray();
        var tree = descendants.OfType<TreeView>().Single();
        var node = ((IEnumerable<NavigationNode>)tree.ItemsSource!).Single();
        Assert.That(node.Title, Is.EqualTo(header));
        Assert.That(descendants.OfType<TextBlock>().Any(x => x.Text == label), Is.True);
    }
}
