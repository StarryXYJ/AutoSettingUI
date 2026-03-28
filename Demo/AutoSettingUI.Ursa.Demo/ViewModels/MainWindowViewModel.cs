using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Ursa.Demo.Models;
using DynamicLocalization.Core;

namespace AutoSettingUI.Ursa.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
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
    private ThemeSettings _themeSettings = new();

    [ObservableProperty]
    private ExtendedControlsSettings _extendedSettings = new();

    [ObservableProperty]
    private ICultureService _localizationService;

    [ObservableProperty]
    private string _currentLanguage = "en";

    public string ViewModeText => IsCustomView ? "Switch to Default View" : "Switch to Custom View";

    partial void OnIsCustomViewChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
    }

    public MainWindowViewModel(ICultureService localizationService)
    {
        LocalizationService = localizationService;

        Targets.Add(ThemeSettings);
        Targets.Add(ExtendedSettings);
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());
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
