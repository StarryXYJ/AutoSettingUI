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

public class PanelTests
{
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
