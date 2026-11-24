using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.WPF.Controls;
using NUnit.Framework;

namespace AutoSettingUI.WPF.Tests;

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
    [Test, Apartment(ApartmentState.STA)]
    public void PanelUpdatesOnUiThreadAndDetaches()
    {
        var service = new Localizer();
        var panel = new WpfAutoSettingPanel
        {
            Targets = new ObservableCollection<object> { new Settings() },
            LocalizationService = service
        };
        var window = new Window { Width = 400, Height = 400, Content = panel, ShowInTaskbar = false };
        try
        {
            window.Show();
            PumpDispatcher();
            Assert.That(service.Subscribers, Is.EqualTo(1));
            CheckPanel(panel, "one-header", "one-label");
            service.Language = "two";
            Task.Run(service.Notify).GetAwaiter().GetResult();
            PumpDispatcher();
            CheckPanel(panel, "two-header", "two-label");
            window.Content = null;
            PumpDispatcher();
            Assert.That(service.Subscribers, Is.Zero);
            service.Language = "three";
            window.Content = panel;
            PumpDispatcher();
            Assert.That(service.Subscribers, Is.EqualTo(1));
            CheckPanel(panel, "three-header", "three-label");
        }
        finally { window.Close(); }
    }

    private static void CheckPanel(WpfAutoSettingPanel panel, string header, string label)
    {
        var nodes = Descendants(panel).ToArray();
        var tree = nodes.OfType<TreeView>().Single();
        var node = ((IEnumerable<NavigationNode>)tree.ItemsSource).Single();
        Assert.That(node.Title, Is.EqualTo(header));
        Assert.That(nodes.OfType<TextBlock>().Any(x => x.Text == label), Is.True);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private static void PumpDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => frame.Continue = false);
        Dispatcher.PushFrame(frame);
    }
}
