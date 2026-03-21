using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using AutoSettingUI.WPF.Controls;
using AutoSettingUI.Wpf.Demo.Models;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Services;
using AutoSettingUI.Wpf.Demo.Resources;

namespace AutoSettingUI.Wpf.Demo;

/// <summary>
/// Main window for the WPF demo application.
/// Demonstrates dynamic language switching, theme management, and settings panel features.
/// </summary>
public partial class MainWindow : Window
{
    private bool _isCustomView = false;
    private ObservableCollection<object> _targets = [];

    private ThemeSettings _themeSettings = new();
    private ExtendedControlsSettings _extendedSettings = new();

    private ILocalizationService? _localizationService;
    private string _currentLanguage = "en";

    /// <summary>
    /// Gets or sets the localization service for dynamic language switching.
    /// When set, applies the service to both settings panels.
    /// </summary>
    public ILocalizationService? LocalizationService
    {
        get => _localizationService;
        set
        {
            _localizationService = value;
            SettingsPanel.LocalizationService = value;
            CustomSettingsPanel.LocalizationService = value;
        }
    }

    /// <summary>
    /// Gets or sets the current language code.
    /// Updates the language button highlighting when changed.
    /// </summary>
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            UpdateLanguageButtons();
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        LocalizationService = new ResxLocalizationService(Strings.ResourceManager);

        SettingsPanel.Targets = _targets;
        SettingsPanel.Targets.Add(_themeSettings);
        SettingsPanel.Targets.Add(_extendedSettings);
        SettingsPanel.Targets.Add(new ApplicationSettings());
        SettingsPanel.Targets.Add(new UserPreferences());
        SettingsPanel.Targets.Add(new NetworkSettings());

        CustomSettingsPanel.Targets = _targets;

        _themeSettings.PropertyChanged += OnThemeSettingsChanged;
    }

    /// <summary>
    /// Updates the font weight of language buttons to highlight the active language.
    /// </summary>
    private void UpdateLanguageButtons()
    {
        BtnEnglish.FontWeight = CurrentLanguage == "en" ? FontWeights.Bold : FontWeights.Normal;
        BtnChinese.FontWeight = CurrentLanguage == "zh-CN" ? FontWeights.Bold : FontWeights.Normal;
    }

    /// <summary>
    /// Handles theme property changes and applies the selected theme to the window.
    /// </summary>
    private void OnThemeSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeSettings.SelectedTheme))
        {
            ApplyTheme(_themeSettings.SelectedTheme);
        }
    }

    /// <summary>
    /// Applies the specified theme to the window background and foreground.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    private void ApplyTheme(AppTheme theme)
    {
        switch (theme)
        {
            case AppTheme.Dark:
                this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                this.Foreground = Brushes.White;
                break;
            case AppTheme.Blue:
                this.Background = new SolidColorBrush(Color.FromRgb(41, 128, 185));
                this.Foreground = Brushes.White;
                break;
            case AppTheme.HighContrast:
                this.Background = Brushes.Black;
                this.Foreground = Brushes.White;
                break;
            default:
                this.Background = new SolidColorBrush(Color.FromRgb(240, 242, 245));
                this.Foreground = Brushes.Black;
                break;
        }
    }

    /// <summary>
    /// Toggles between default and custom styled settings panel views.
    /// </summary>
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

    /// <summary>
    /// Toggles the visibility of the navigation sidebar.
    /// </summary>
    private void ToggleNav_Click(object sender, RoutedEventArgs e)
    {
        _showNavigation = !_showNavigation;
        SettingsPanel.ShowNavigation = _showNavigation;
        CustomSettingsPanel.ShowNavigation = _showNavigation;
        ToggleNavButton.Content = $"Nav: {_showNavigation}";
    }

    /// <summary>
    /// Adds a new ApplicationSettings instance to the targets collection.
    /// </summary>
    private void AddApp(object sender, RoutedEventArgs e)
    {
        _targets.Add(new ApplicationSettings() { AppName = "New App", Version = "2.0.0" });
    }

    /// <summary>
    /// Adds a new UserPreferences instance to the targets collection.
    /// </summary>
    private void AddUser(object sender, RoutedEventArgs e)
    {
        _targets.Add(new UserPreferences() { Language = "Spanish", FontSize = 16 });
    }

    /// <summary>
    /// Adds a new NetworkSettings instance to the targets collection.
    /// </summary>
    private void AddNetworkConfig(object sender, RoutedEventArgs e)
    {
        _targets.Add(new NetworkSettings() { Port = 8080 });
    }

    /// <summary>
    /// Switches the application language to English.
    /// </summary>
    private void SwitchToEnglish(object sender, RoutedEventArgs e)
    {
        LocalizationService?.SetCulture("en");
        CurrentLanguage = "en";
    }

    /// <summary>
    /// Switches the application language to Chinese (Simplified).
    /// </summary>
    private void SwitchToChinese(object sender, RoutedEventArgs e)
    {
        LocalizationService?.SetCulture("zh-CN");
        CurrentLanguage = "zh-CN";
    }
}
