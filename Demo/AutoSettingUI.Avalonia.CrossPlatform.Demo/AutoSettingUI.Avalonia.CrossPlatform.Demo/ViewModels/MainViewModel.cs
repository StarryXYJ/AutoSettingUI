using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Avalonia.CrossPlatform.Demo.Models;
using Avalonia;
using Avalonia.Styling;
using DynamicLocalization.Core;

namespace AutoSettingUI.Avalonia.CrossPlatform.Demo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<object> _targets = new();

    [ObservableProperty]
    private string _title = "Settings";

    [ObservableProperty]
    private bool _showNavigation = true;

    [ObservableProperty]
    private double _navigationWidth = 200;

    [ObservableProperty]
    private bool _isCustomView = false;

    [ObservableProperty]
    private ExtendedControlsSettings _extendedSettings = new();

    [ObservableProperty]
    private ThemeSettings _themeSettings = new();

    [ObservableProperty]
    private ICultureService _localizationService;

    [ObservableProperty]
    private string _currentLanguage = "en";

    public string ViewModeText => IsCustomView ? "Switch to Default View" : "Switch to Custom View";

    partial void OnIsCustomViewChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
    }

    public MainViewModel(ICultureService localizationService)
    {
        LocalizationService = localizationService;

        Targets.Add(ThemeSettings);
        Targets.Add(ExtendedSettings);
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());

        ThemeSettings.PropertyChanged += OnThemeSettingsChanged;
    }

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

    [RelayCommand]
    private void AddApp()
    {
        Targets.Add(new ApplicationSettings() { AppName = "New App", Version = "2.0.0" });
    }

    [RelayCommand]
    private void AddUser()
    {
        Targets.Add(new UserPreferences() { Language = "Spanish", FontSize = 16 });
    }

    [RelayCommand]
    private void AddNetworkConfig()
    {
        Targets.Add(new NetworkSettings() { Port = 8080 });
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsCustomView = !IsCustomView;
    }

    [RelayCommand]
    private void ToggleNavigation()
    {
        ShowNavigation = !ShowNavigation;
    }

    [RelayCommand]
    private void SwitchToEnglish()
    {
        LocalizationService.SetCulture("en");
        CurrentLanguage = "en";
    }

    [RelayCommand]
    private void SwitchToChinese()
    {
        LocalizationService.SetCulture("zh-CN");
        CurrentLanguage = "zh-CN";
    }
}

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
