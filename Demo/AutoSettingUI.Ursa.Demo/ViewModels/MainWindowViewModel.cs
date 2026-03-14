using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoSettingUI.Ursa.Demo.Models;
using Ursa.Themes.Semi;

namespace AutoSettingUI.Ursa.Demo.ViewModels;

/// <summary>
/// Represents a theme option with display name and ThemeVariant.
/// </summary>
public record ThemeInfo(string Name, ThemeVariant Theme);

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
    private ThemeInfo _selectedThemeInfo;

    public string ViewModeText => IsCustomView ? "Switch to Default View" : "Switch to Custom View";

    // Available themes for the ComboBox
    public List<ThemeInfo> AvailableThemes { get; } = new()
    {
        new ThemeInfo("Light", ThemeVariant.Light),
        new ThemeInfo("Dark", ThemeVariant.Dark),
        new ThemeInfo("Dusk", SemiTheme.Dusk),
        new ThemeInfo("NightSky", SemiTheme.NightSky),
        new ThemeInfo("Aquatic", SemiTheme.Aquatic),
        new ThemeInfo("Desert", SemiTheme.Desert),
        new ThemeInfo("Default (System)", ThemeVariant.Default)
    };

    partial void OnIsCustomViewChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
    }

    partial void OnSelectedThemeInfoChanged(ThemeInfo value)
    {
        // Apply the theme to the application
        var app = Application.Current;
        if (app is null) return;

        app.RequestedThemeVariant = value.Theme;

        
    }

    public MainWindowViewModel()
    {
        // Initialize with default settings
        Targets.Add(new ApplicationSettings());
        Targets.Add(new UserPreferences());
        Targets.Add(new NetworkSettings());

        // Set default theme (Dark)
        SelectedThemeInfo = AvailableThemes[1];
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