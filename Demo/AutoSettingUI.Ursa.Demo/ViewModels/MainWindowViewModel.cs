using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Ursa.Demo.Models;

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

    public string ViewModeText => IsCustomView ? "Switch to Default View" : "Switch to Custom View";

    partial void OnIsCustomViewChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
    }

    public MainWindowViewModel()
    {
        // Initialize with default settings
        Targets.Add(ThemeSettings);
        Targets.Add(ExtendedSettings);
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());

        // Subscribe to theme changes - the theme is now applied directly in ThemeSettings.ApplyTheme
        ThemeSettings.ThemeChanged += (s, theme) =>
        {
            // Theme is already applied in ThemeSettings.ApplyTheme
            // This event can be used for additional handling if needed
        };
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
}