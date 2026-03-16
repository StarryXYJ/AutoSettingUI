using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
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

    private ThemeSettings _themeSettings = new();
    private ExtendedControlsSettings _extendedSettings = new();

    public MainWindow()
    {
        InitializeComponent();

        // Default settings
        SettingsPanel.Targets = _targets;
        SettingsPanel.Targets.Add(_themeSettings);
        SettingsPanel.Targets.Add(_extendedSettings);
        SettingsPanel.Targets.Add(new ApplicationSettings());
        SettingsPanel.Targets.Add(new UserPreferences());
        SettingsPanel.Targets.Add(new NetworkSettings());

        CustomSettingsPanel.Targets= _targets;
        CustomSettingsPanel.Targets.Add(_themeSettings);
        CustomSettingsPanel.Targets.Add(_extendedSettings);
        CustomSettingsPanel.Targets.Add(new ApplicationSettings());
        CustomSettingsPanel.Targets.Add(new UserPreferences());
        CustomSettingsPanel.Targets.Add(new NetworkSettings());

        _themeSettings.PropertyChanged += OnThemeSettingsChanged;
    }

    private void OnThemeSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeSettings.SelectedTheme))
        {
            ApplyTheme(_themeSettings.SelectedTheme);
        }
    }

    private void ApplyTheme(string themeName)
    {
        // Simple theme switching via colors for premium look
        switch (themeName)
        {
            case "Dark":
                this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                this.Foreground = Brushes.White;
                break;
            case "Blue":
                this.Background = new SolidColorBrush(Color.FromRgb(41, 128, 185));
                this.Foreground = Brushes.White;
                break;
            default:
                this.Background = Brushes.White;
                this.Foreground = Brushes.Black;
                break;
        }
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