using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Ursa.Attributes;
using Avalonia.Controls;
using Avalonia.Media;
using Ursa.Controls;
using DescriptionAttribute = AutoSettingUI.Core.Attributes.DescriptionAttribute;
using ReadOnlyAttribute = AutoSettingUI.Core.Attributes.ReadOnlyAttribute;
using Avalonia.Styling;
using Ursa.Themes.Semi;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSettingUI.Ursa.Demo.Models;

[SettingUI]
[MainHeader("Settings.Application", UseResourceKey = true)]
public partial class ApplicationSettings:ObservableObject
{
    
    [Title("Settings.Application", UseResourceKey = true)]
    [ObservableProperty]
    [DisplayOrder(-1)]
    private string _appName= "My Application";

    [ObservableProperty]
    [DisplayOrder(-1)]
    private string _version="1.0.0";

    [Title("Settings.EnableLogging", UseResourceKey = true)]
    [ObservableProperty]
    [DisplayOrder(1)]
    private bool _enableLogging;

    [Title("Settings.LogLevel", UseResourceKey = true)]
    [Hide(nameof(ShouldHideLogging))]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [Title("Settings.MaxLogSize", UseResourceKey = true)]
    [Range(1, 100)]
    [Hide(nameof(ShouldHideLogging))]
    public int MaxLogSize { get; set; } = 10;

    [Title("Settings.Volume", UseResourceKey = true)]
    [ControlBinding(typeof(global::Avalonia.Controls.Slider),"Value",nameof(VolumeFactory))]
    public double Volume { get; set; } = 50.0;
    public Slider VolumeFactory()
    {
        var slider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            MaxWidth = 100,
            Background = new SolidColorBrush(Colors.Aqua)
        };
        return slider;
    }
    [Hide]
    public string InternalId { get; set; } = Guid.NewGuid().ToString();

    public bool ShouldHideLogging => !EnableLogging;

    
}

/// <summary>
/// Sample user preferences class demonstrating delegate properties and collection editors.
/// </summary>
[SettingUI]

public partial class UserPreferences : ObservableObject
{
    private string _email = "";
    
    [SubHeader("Display")]
    [Title("Theme")]
    public Theme Theme { get; set; } = Theme.Light;

    [Title("Language")]
    public string Language { get; set; } = "English";

    [Title("Font Size")]
    [NumericUpDown(Minimum = 8, Maximum = 32, Increment = 1)]
    public int FontSize { get; set; } = 14;

    [SubHeader("Actions (Delegate Properties)")]
    [Title("Reset Settings")]
    public Action? ResetSettingsCommand { get; set; }

    [Title("Export Settings")]
    [CommandCanExecute(nameof(CanExport))]
    public Action? ExportSettingsCommand { get; set; }

    [Title("Import Settings")]
    [CommandCanExecute(nameof(CanImport))]
    public Action? ImportSettingsCommand { get; set; }

    [Title("Admin Action")]
    [CommandCanExecute(nameof(IsAdmin))]
    public Action? AdminActionCommand { get; set; }

    [SubHeader("ReadOnly Examples")]
    [Title("Read-Only Field")]
    [ReadOnly(true)]
    [ObservableProperty]
    private string _readOnlyField  = "This field is read-only";

    [Title("Dynamic ReadOnly")]
    [ReadOnly(nameof(CanEdit))]
    [ObservableProperty]
    private string _dynamicReadOnlyField= "Only editable by admins";

    [Title("Dynamic ReadOnly")]
    [ReadOnly(nameof(CanEdit))]
    [ObservableProperty]
    [NumericUpDown(Minimum = 0, Maximum = 100, Increment = 1)]
    private int _dynamicReadOnlyFieldInt= 42;

    [ControlBinding(typeof(CheckBox), "IsChecked")]
    [ObservableProperty]
    private bool _isAdmin = true;

    [SubHeader("Notifications")]
    [Title("Enable Notifications")]
    [ObservableProperty]
    private bool _enableNotifications = true;

    [Title("Notification Sound")]
    [Hide(nameof(ShouldHideNotifications))]
    public bool NotificationSound { get; set; } = true;

    [Title("Notification Email")]
    [Description("Email address for sending notifications")]
    [Hide(nameof(ShouldHideNotifications))]
    [Placeholder("notifications@example.com")]
    public string NotificationEmail { get; set; } = "";

    public bool ShouldHideNotifications => !EnableNotifications;

    [Title("Email Address")]
    [Description("Your email address for notifications and account recovery")]
    [Placeholder("example@domain.com")]
    [Validation(Required = true, ErrorMessage = "Email is required")]
    [Validation(Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
    [Layout(Width = 250, HorizontalAlignment = "Stretch")]
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    [SubHeader("Layout & Validation Examples")]

    // Username: Required, length validation, custom layout
    [Title("Username")]
    [Description("Your display name (3-20 characters)")]
    [Placeholder("Enter username")]
    [Validation(Required = true, MinLength = 3, MaxLength = 20, ErrorMessage = "Username must be 3-20 characters")]
    [Layout(Width = 200, Height = 58, Margin = "0,2,0,2")]
    public string Username { get; set; } = "";

    // Password: Custom mask character
    [Title("Password")]
    [Password]  // Default mask character '•'
    [Validation(Required = true, MinLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [Layout(Width = 200)]
    public string Password { get; set; } = "";

    // API Key: Different mask character
    [Title("API Key")]
    [Password('*')]  // Custom mask character '*'
    [Placeholder("Enter API key")]
    [Layout(Width = 300)]
    public string ApiKey { get; set; } = "";

    // Age: Numeric range validation
    [Title("Age")]
    [Description("Your age in years")]
    [Validation(MinValue = 0, MaxValue = 150, ErrorMessage = "Age must be between 0 and 150")]
    [Layout(Width = 80)]
    public int Age { get; set; } = 25;

    // Website: Regex pattern validation
    [Title("Website")]
    [Description("Your personal website URL")]
    [Placeholder("https://example.com")]
    [Validation(Pattern = @"^https?://.*", ErrorMessage = "URL must start with http:// or https://")]
    [Layout(MinWidth = 200, MaxWidth = 400, HorizontalAlignment = "Stretch")]
    public string Website { get; set; } = "";

    [SubHeader("Collection Examples")]
    [Title("Tags (Default Collection Editor)")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedTag))]
    public ObservableCollection<string> Tags { get; set; } =  [ "Important", "Work" ];

    [Title("Selected Tag")]
    [Description("The currently selected tag from the list above")]
    [ObservableProperty]
    private string? _selectedTag;

    [Title("Versions (Read-Only Collection)")]
    [CollectionEditor(AllowAdd = false, AllowRemove = false, AllowReorder = false)]
    public ObservableCollection<string> Versions { get; set; } = ["1.0.0", "1.1.0", "2.0.0"];

    [Title("People (Complex Collection)")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedPerson))]
    public ObservableCollection<Person> People { get; set; } =
    [
        new Person { Name = "John Doe", Age = 30, Email = "john@example.com" },
        new Person { Name = "Jane Smith", Age = 25, Email = "jane@example.com" }
    ];

    [Title("Selected Person")]
    [Description("The currently selected person - edit details below")]
    [ObservableProperty]
    private Person? _selectedPerson;

    public UserPreferences()
    {
        ResetSettingsCommand = ResetSettings;
        ExportSettingsCommand = ExportSettings;
        ImportSettingsCommand = ImportSettings;
        AdminActionCommand = AdminAction;
    }

    private void ResetSettings()
    {
        Theme = Theme.Light;
        Language = "English";
        FontSize = 14;
        EnableNotifications = true;
        NotificationSound = true;
        Console.WriteLine("Settings have been reset!");
    }

    private async void ExportSettings()
    {
        await MessageBox.ShowAsync("Exporting settings...", "Export");
    }

    private async void ImportSettings()
    {
        await MessageBox.ShowAsync( "Importing settings...", "Import");
    }

    private async void AdminAction()
    {
        await MessageBox.ShowAsync( "Performing admin action...", "Admin Action");
    }

    public bool CanExport() => EnableNotifications;
    public bool CanImport() => !string.IsNullOrEmpty(Email);
    public bool CanEdit() => !IsAdmin;
}

/// <summary>
/// Sample network settings class.
/// </summary>
[SettingUI]
[MainHeader("Settings.Network", UseResourceKey = true)]
public class NetworkSettings
{
    [SubHeader("Connection")]
    [Title("Settings.ServerAddress", UseResourceKey = true)]
    public string ServerAddress { get; set; } = "localhost";

    [Title("Settings.Port", UseResourceKey = true)]
    [Range(1, 65535)]
    public int Port { get; set; } = 8080;

    [Title("Settings.UseHttps", UseResourceKey = true)]
    [DisplayOrder(-1)]
    public bool UseHttps { get; set; } = false;

    [SubHeader("Authentication")]
    [Title("Settings.Username", UseResourceKey = true)]
    [DisplayOrder(1)]
    public string Username { get; set; } = "";

    [Title("Settings.Timeout", UseResourceKey = true)]
    [Range(1, 300)]
    [DisplayOrder(1)]
    public int Timeout { get; set; } = 30;

    [SubHeader("Actions")]
    [Title("Settings.TestConnection", UseResourceKey = true)]
    [CommandCanExecute(nameof(CanTestConnection))]
    public Action? TestConnectionCommand { get; set; }

    [Title("Settings.PingServer", UseResourceKey = true)]
    public Action? PingServerCommand { get; set; }

    public NetworkSettings()
    {
        TestConnectionCommand = TestConnection;
        PingServerCommand = PingServer;
    }

    private void TestConnection()
    {
        Console.WriteLine($"Testing connection to {ServerAddress}:{Port}...");
    }

    private void PingServer()
    {
        Console.WriteLine($"Pinging {ServerAddress}...");
    }

    public bool CanTestConnection() => !string.IsNullOrEmpty(ServerAddress) && Port > 0;
}

/// <summary>
/// Log level enumeration.
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

/// <summary>
/// Theme enumeration.
/// </summary>
public enum Theme
{
    Light,
    Dark,
    System
}
/// <summary>
/// Sample class demonstrating extended controls like ColorPicker, IPv4Box, etc.
/// </summary>
[SettingUI]
[MainHeader("Settings.ExtendedControls", UseResourceKey = true)]
public partial class ExtendedControlsSettings  : ObservableObject
{
    [Title("Settings.BackgroundColor", UseResourceKey = true)]
    [ColorPicker]
    public Color ThemeColor { get; set; } = Colors.DodgerBlue;

    [Title("Settings.ServerIP", UseResourceKey = true)]
    [IPv4Box]
    public IPAddress ServerIP { get; set; } = IPAddress.Parse("192.168.1.1");

    [Title("Settings.SelectionTags", UseResourceKey = true)]
    [TagInput]
    public ObservableCollection<string> ProjectTags { get; set; } = ["Ursa", "Avalonia", "AutoSettingUI"];
    

    [Title("Settings.ReleaseDate", UseResourceKey = true)]
    [DatePicker]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [Title("Settings.PreferredTime", UseResourceKey = true)]
    [TimePicker]
    public TimeSpan PreferredTime { get; set; } = DateTime.Now.TimeOfDay;

    [CheckBox]
    public bool EnableAdvancedFeature { get; set; } = true;

    [Title("Settings.ItemCount", UseResourceKey = true)]
    [NumericUpDown(Minimum = 0, Maximum = 100, Increment = 5)]
    [ObservableProperty]
    [DisplayOrder(1)]
    private int _itemCount = 40;

    #region ControlBinding Examples

    [SubHeader("ControlBinding Examples")]

    // Example 1: Auto-bind with BindingProperty specified
    [Title("Progress (Auto-bind)")]
    [Description("Uses ControlBinding with BindingProperty - auto binds to Value")]
    [ControlBinding(typeof(global::Avalonia.Controls.ProgressBar), "Value")]
    public double ProgressValue { get; set; } = 50.0;

    // Example 2: Factory method with custom binding (no BindingProperty)
    [Title("Custom Slider (Factory Method)")]
    [Description("Factory method handles binding manually - can customize converter, etc.")]
    [ControlBinding(typeof(global::Avalonia.Controls.Slider), FactoryMethod = nameof(CreateCustomSlider))]
    public double CustomSliderValue { get; set; } = 75.0;

    public global::Avalonia.Controls.Slider CreateCustomSlider()
    {
        var slider = new global::Avalonia.Controls.Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 200
        };
        
        // Custom binding with converter
        slider.Bind(global::Avalonia.Controls.Slider.ValueProperty, 
            new global::Avalonia.Data.Binding(nameof(CustomSliderValue)) 
            { 
                Source = this,
                Mode = global::Avalonia.Data.BindingMode.TwoWay
            });
        
        return slider;
    }

    // Example 3: Control type only - auto-detect binding
    [Title("Auto-detected TextBox")]
    [Description("Only control type specified - auto-detects Text property")]
    [ControlBinding(typeof(global::Avalonia.Controls.TextBox))]
    public string AutoDetectedText { get; set; } = "Auto-detected binding";

    // Example 4: Factory method with complex control setup
    [Title("Rating (Complex Factory)")]
    [Description("Factory creates a custom rating control with full customization")]
    [ControlBinding(typeof(global::Avalonia.Controls.StackPanel), FactoryMethod = nameof(CreateRatingControl))]
    [ObservableProperty]
    private int _rating = 3;

    public global::Avalonia.Controls.StackPanel CreateRatingControl()
    {
        var panel = new global::Avalonia.Controls.StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 5
        };

        for (int i = 1; i <= 5; i++)
        {
            var starIndex = i;
            var button = new global::Avalonia.Controls.Button
            {
                Content = "★",
                FontSize = 24,
                Width = 40,
                Height = 40
            };

            button.Click += (s, e) => Rating = starIndex;
            
            // Update button appearance based on rating
            button.Bind(global::Avalonia.Controls.Button.ForegroundProperty,
                new global::Avalonia.Data.Binding(nameof(Rating))
                {
                    Source = this,
                    Converter = new RatingConverter(starIndex)
                });

            panel.Children.Add(button);
        }

        return panel;
    }

    #endregion
}

/// <summary>
/// Theme enumeration for application theming.
/// </summary>
public enum AppTheme
{
    Default,
    Light,
    Dark,
    Dusk,
    NightSky,
    Aquatic,
    Desert
}

/// <summary>
/// Converter for rating control - highlights stars based on rating value.
/// </summary>
public class RatingConverter : global::Avalonia.Data.Converters.IValueConverter
{
    private readonly int _starIndex;

    public RatingConverter(int starIndex)
    {
        _starIndex = starIndex;
    }

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int rating)
        {
            return rating >= _starIndex 
                ? global::Avalonia.Media.Brushes.Gold 
                : global::Avalonia.Media.Brushes.Gray;
        }
        return global::Avalonia.Media.Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Settings class for theme management within the form.
/// </summary>
[SettingUI]
[MainHeader("Settings.Theme", UseResourceKey = true)]
public partial class ThemeSettings : ObservableObject
{
    private AppTheme _selectedTheme = AppTheme.Default;

    public event EventHandler<AppTheme>? ThemeChanged;

    [Title("Settings.SelectedTheme", UseResourceKey = true)]
    [Description("Settings.SelectedThemeDesc", UseResourceKey = true)]
    [ItemsSource(typeof(ThemeSettings), nameof(AvailableThemes))]
    public AppTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                ApplyTheme(value);
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public static List<AppTheme> AvailableThemes => new()
    {
        AppTheme.Default,
        AppTheme.Light,
        AppTheme.Dark,
        AppTheme.Dusk,
        AppTheme.NightSky,
        AppTheme.Aquatic,
        AppTheme.Desert
    };

    private void ApplyTheme(AppTheme theme)
    {
        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.RequestedThemeVariant = theme switch
            {
                AppTheme.Light => ThemeVariant.Light,
                AppTheme.Dark => ThemeVariant.Dark,
                AppTheme.Dusk => global::Ursa.Themes.Semi.SemiTheme.Dusk,
                AppTheme.NightSky => global::Ursa.Themes.Semi.SemiTheme.NightSky,
                AppTheme.Aquatic => global::Ursa.Themes.Semi.SemiTheme.Aquatic,
                AppTheme.Desert => global::Ursa.Themes.Semi.SemiTheme.Desert,
                _ => ThemeVariant.Default
            };
        }
    }
}
