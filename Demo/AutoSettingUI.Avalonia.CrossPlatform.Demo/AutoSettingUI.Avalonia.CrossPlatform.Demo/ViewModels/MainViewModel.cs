using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Avalonia.CrossPlatform.Demo.Models;
using AutoSettingUI.Avalonia.CrossPlatform.Demo.Resources;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Services;
using Avalonia;
using Avalonia.Styling;

namespace AutoSettingUI.Avalonia.CrossPlatform.Demo.ViewModels;

/// <summary>
/// Main view model for the Avalonia cross-platform demo application.
/// Demonstrates dynamic language switching, theme management, and settings panel features.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    /// <summary>
    /// Collection of setting objects to display in the settings panel.
    /// Each object represents a different settings category.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<object> _targets = new();

    /// <summary>
    /// The title displayed at the top of the settings panel.
    /// </summary>
    [ObservableProperty]
    private string _title = "Settings";

    /// <summary>
    /// Controls whether the navigation sidebar is visible.
    /// </summary>
    [ObservableProperty]
    private bool _showNavigation = true;

    /// <summary>
    /// Width of the navigation sidebar in pixels.
    /// </summary>
    [ObservableProperty]
    private double _navigationWidth = 200;

    /// <summary>
    /// Toggles between default and custom styled views.
    /// </summary>
    [ObservableProperty]
    private bool _isCustomView = false;

    /// <summary>
    /// Settings for extended UI controls demonstration.
    /// </summary>
    [ObservableProperty]
    private ExtendedControlsSettings _extendedSettings = new();

    /// <summary>
    /// Settings for theme selection.
    /// </summary>
    [ObservableProperty]
    private ThemeSettings _themeSettings = new();

    /// <summary>
    /// Localization service for dynamic language switching.
    /// </summary>
    [ObservableProperty]
    private ILocalizationService? _localizationService;

    /// <summary>
    /// Current language code (e.g., "en", "zh-CN").
    /// Used to highlight the active language button.
    /// </summary>
    [ObservableProperty]
    private string _currentLanguage = "en";

    /// <summary>
    /// Dynamic text for the view toggle button.
    /// </summary>
    public string ViewModeText => IsCustomView ? "Switch to Default View" : "Switch to Custom View";

    partial void OnIsCustomViewChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
    }

    public MainViewModel()
    {
        LocalizationService = new ResxLocalizationService(Strings.ResourceManager);

        Targets.Add(ThemeSettings);
        Targets.Add(ExtendedSettings);
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());

        ThemeSettings.PropertyChanged += OnThemeSettingsChanged;
    }

    /// <summary>
    /// Handles theme changes and applies them to the application.
    /// </summary>
    private void OnThemeSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeSettings.SelectedTheme))
        {
            var app = Application.Current;
            if (app != null)
            {
                app.RequestedThemeVariant = ThemeSettings.SelectedTheme switch
                {
                    AppTheme.Light => ThemeVariant.Light,
                    AppTheme.Dark => ThemeVariant.Dark,
                    _ => ThemeVariant.Default
                };
            }
        }
    }

    /// <summary>
    /// Adds a new ApplicationSettings instance to the targets collection.
    /// </summary>
    [RelayCommand]
    private void AddApp()
    {
        Targets.Add(new ApplicationSettings() { AppName = "New App", Version = "2.0.0" });
    }

    /// <summary>
    /// Adds a new UserPreferences instance to the targets collection.
    /// </summary>
    [RelayCommand]
    private void AddUser()
    {
        Targets.Add(new UserPreferences() { Language = "Spanish", FontSize = 16 });
    }

    /// <summary>
    /// Adds a new NetworkSettings instance to the targets collection.
    /// </summary>
    [RelayCommand]
    private void AddNetworkConfig()
    {
        Targets.Add(new NetworkSettings() { Port = 8080 });
    }

    /// <summary>
    /// Toggles between default and custom styled settings panel views.
    /// </summary>
    [RelayCommand]
    private void ToggleView()
    {
        IsCustomView = !IsCustomView;
    }

    /// <summary>
    /// Toggles the visibility of the navigation sidebar.
    /// </summary>
    [RelayCommand]
    private void ToggleNavigation()
    {
        ShowNavigation = !ShowNavigation;
    }

    /// <summary>
    /// Switches the application language to English.
    /// </summary>
    [RelayCommand]
    private void SwitchToEnglish()
    {
        LocalizationService?.SetCulture("en");
        CurrentLanguage = "en";
    }

    /// <summary>
    /// Switches the application language to Chinese (Simplified).
    /// </summary>
    [RelayCommand]
    private void SwitchToChinese()
    {
        LocalizationService?.SetCulture("zh-CN");
        CurrentLanguage = "zh-CN";
    }
}

/// <summary>
/// Value converter that highlights the currently selected language button.
/// Returns Bold font weight for the active language, Normal for others.
/// </summary>
/// <example>
/// Usage in XAML:
/// TextBlock FontWeight="{Binding CurrentLanguage, Converter={StaticResource LanguageToFontWeightConverter}, ConverterParameter=en}"
/// </example>
public class LanguageToFontWeightConverter : global::Avalonia.Data.Converters.IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string currentLang && parameter is string targetLang)
        {
            return currentLang == targetLang ? global::Avalonia.Media.FontWeight.Bold : global::Avalonia.Media.FontWeight.Normal;
        }
        return global::Avalonia.Media.FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
