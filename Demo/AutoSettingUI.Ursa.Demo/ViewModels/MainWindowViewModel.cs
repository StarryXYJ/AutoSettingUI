using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Ursa.Demo.Models;
using AutoSettingUI.Ursa.Demo.Resources;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Services;

namespace AutoSettingUI.Ursa.Demo.ViewModels;

/// <summary>
/// Main view model for the Ursa demo application.
/// Demonstrates dynamic language switching, theme management, and Ursa-specific controls.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
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
    /// Settings for theme selection with Ursa-specific themes.
    /// </summary>
    [ObservableProperty]
    private ThemeSettings _themeSettings = new();

    /// <summary>
    /// Settings for extended UI controls demonstration (ColorPicker, TagInput, IPv4Box, etc.).
    /// </summary>
    [ObservableProperty]
    private ExtendedControlsSettings _extendedSettings = new();

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

    public MainWindowViewModel()
    {
        LocalizationService = new ResxLocalizationService(Strings.ResourceManager);
        
        Targets.Add(ThemeSettings);
        Targets.Add(ExtendedSettings);
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());

        ThemeSettings.ThemeChanged += (s, theme) =>
        {
        };
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
public class LanguageToFontWeightConverter : Avalonia.Data.Converters.IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string currentLang && parameter is string targetLang)
        {
            return currentLang == targetLang ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
        }
        return Avalonia.Media.FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
