using System.Collections.ObjectModel;
using System.Windows;

using AutoSettingUI.WPF.Controls;
using AutoSettingUI.Wpf.Demo.Models;

namespace AutoSettingUI.Wpf.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isCustomView = false;
    private ObservableCollection<object> _targets = [];

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Create sample settings instances
        var appSettings = new ApplicationSettings();
        var userPreferences = new UserPreferences();
        var networkSettings = new NetworkSettings();

        // Set the targets for both panels
        // Note: WPF's CommandManager automatically handles CanExecute re-evaluation
        // when UI changes occur (focus changes, text input, etc.)
        _targets = [appSettings, userPreferences, networkSettings];
        SettingsPanel.Targets = _targets;
        CustomSettingsPanel.Targets = _targets;
    }

    private void ToggleView_Click(object sender, RoutedEventArgs e)
    {
        _isCustomView = !_isCustomView;
        
        if (_isCustomView)
        {
            SettingsPanel.Visibility = Visibility.Collapsed;
            CustomSettingsPanel.Visibility = Visibility.Visible;
            ToggleViewButton.Content = "Switch to Default View";
        }
        else
        {
            SettingsPanel.Visibility = Visibility.Visible;
            CustomSettingsPanel.Visibility = Visibility.Collapsed;
            ToggleViewButton.Content = "Switch to Custom View";
        }
    }

    private bool _showNavigation = true;

    private void ToggleNav_Click(object sender, RoutedEventArgs e)
    {
        _showNavigation = !_showNavigation;
        SettingsPanel.ShowNavigation = _showNavigation;
        CustomSettingsPanel.ShowNavigation = _showNavigation;
        ToggleNavButton.Content = $"Nav: {_showNavigation}";
    }

    private void AddApp(object sender, RoutedEventArgs e)
    {
        _targets.Add(new ApplicationSettings(){AppName = "New App", Version = "2.0.0"});
    }

    private void AddUser(object sender, RoutedEventArgs e)
    {
        _targets.Add(new UserPreferences(){Language = "Spanish", FontSize = 16});
    }

    private void AddNetworkConfig(object sender, RoutedEventArgs e)
    {
        _targets.Add(new NetworkSettings(){ Port = 8080});
    }
}