using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Avalonia.Attributes;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using DescriptionAttribute = AutoSettingUI.Core.Attributes.DescriptionAttribute;
using ReadOnlyAttribute = AutoSettingUI.Core.Attributes.ReadOnlyAttribute;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSettingUI.Avalonia.CrossPlatform.Demo.Models;

[SettingUI]
[MainHeader("Settings.Application", UseResourceKey = true)]
public partial class ApplicationSettings : ObservableObject
{
    [Title("Settings.Application", UseResourceKey = true)]
    [ObservableProperty]
    [DisplayOrder(-1)]
    private string _appName = "My Application";

    [ObservableProperty]
    [DisplayOrder(-1)]
    private string _version = "1.0.0";

    [Title("Settings.EnableLogging", UseResourceKey = true)]
    [ObservableProperty]
    [DisplayOrder(1)]
    private bool _enableLogging;

    [Title("Settings.LogLevel", UseResourceKey = true)]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [Title("Settings.MaxLogSize", UseResourceKey = true)]
    [Range(1, 100)]
    public int MaxLogSize { get; set; } = 10;

    [Title("Settings.Volume", UseResourceKey = true)]
    [ControlBinding(typeof(global::Avalonia.Controls.Slider), "Value", nameof(VolumeFactory))]
    public double Volume { get; set; } = 50.0;

    [Hide]
    public string InternalId { get; set; } = Guid.NewGuid().ToString();

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
}

[SettingUI]
[MainHeader("User Preferences")]
public partial class UserPreferences : ObservableObject
{
    private bool _isAdmin = true;
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
    public string ReadOnlyField { get; set; } = "This field is read-only";

    [Title("Dynamic ReadOnly")]
    [ReadOnly(nameof(CanEdit))]
    public string DynamicReadOnlyField { get; set; } = "Only editable by admins";

    [ControlBinding(typeof(CheckBox), "IsChecked")]
    public bool IsAdmin
    {
        get => _isAdmin;
        set
        {
            if (_isAdmin != value)
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }
    }

    [SubHeader("Notifications")]
    [Title("Enable Notifications")]
    public bool EnableNotifications { get; set; } = true;

    [Title("Notification Sound")]
    public bool NotificationSound { get; set; } = true;

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
    [Title("Username")]
    [Description("Your display name (3-20 characters)")]
    [Placeholder("Enter username")]
    [Validation(Required = true, MinLength = 3, MaxLength = 20, ErrorMessage = "Username must be 3-20 characters")]
    [Layout(Width = 200, Height = 28, Margin = "0,2,0,2")]
    public string Username { get; set; } = "";

    [Title("Password")]
    [Password]
    [Validation(Required = true, MinLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [Layout(Width = 200)]
    public string Password { get; set; } = "";

    [Title("API Key")]
    [Password('*')]
    [Placeholder("Enter API key")]
    [Layout(Width = 300)]
    public string ApiKey { get; set; } = "";

    [Title("Age")]
    [Description("Your age in years")]
    [Validation(MinValue = 0, MaxValue = 150, ErrorMessage = "Age must be between 0 and 150")]
    [Layout(Width = 80)]
    public int Age { get; set; } = 25;

    [Title("Website")]
    [Description("Your personal website URL")]
    [Placeholder("https://example.com")]
    [Validation(Pattern = @"^https?://.*", ErrorMessage = "URL must start with http:// or https://")]
    [Layout(MinWidth = 200, MaxWidth = 400, HorizontalAlignment = "Stretch")]
    public string Website { get; set; } = "";

    [SubHeader("Collection Examples")]
    [Title("Tags (Default Collection Editor)")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedTag))]
    public ObservableCollection<string> Tags { get; set; } = ["Important", "Work"];

    [Title("Selected Tag")] [Description("The currently selected tag from the list above")]
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

    [Title("Selected Person")] [Description("The currently selected person - edit details below")] [ObservableProperty]
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

    private void ExportSettings()
    {
        Console.WriteLine("Settings exported!");
    }

    private void ImportSettings()
    {
        Console.WriteLine("Settings imported!");
    }

    private void AdminAction()
    {
        Console.WriteLine("Admin action executed!");
    }

    public bool CanExport() => EnableNotifications;
    public bool CanImport() => !string.IsNullOrEmpty(Email);
    public bool CanEdit() => !IsAdmin;
}

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

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public enum Theme
{
    Light,
    Dark,
    System
}

[SettingUI]
[MainHeader("Settings.ExtendedControls", UseResourceKey = true)]
public partial class ExtendedControlsSettings : ObservableObject
{
    [Title("Settings.BackgroundColor", UseResourceKey = true)]
    [ColorPicker]
    public Color ThemeColor { get; set; } = Colors.DodgerBlue;

    [Title("Settings.ReleaseDate", UseResourceKey = true)]
    [DatePicker]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [Title("Settings.PreferredTime", UseResourceKey = true)]
    [TimePicker]
    public TimeSpan PreferredTime { get; set; } = DateTime.Now.TimeOfDay;

    [CheckBox]
    public bool EnableAdvancedFeature { get; set; } = true;

    [Title("Settings.ItemCount", UseResourceKey = true)]
    [NumericUpDown(Minimum = 0, Maximum = 100, Increment = 10)]
    [ObservableProperty]
    [DisplayOrder(1)]
    private int _itemCount = 40;
}

public enum AppTheme
{
    Default,
    Light,
    Dark
}

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

    public static ObservableCollection<AppTheme> AvailableThemes => new()
    {
        AppTheme.Default,
        AppTheme.Light,
        AppTheme.Dark
    };

    private void ApplyTheme(AppTheme theme)
    {
        if (global::Avalonia.Application.Current is not null)
        {
            global::Avalonia.Application.Current.RequestedThemeVariant = theme switch
            {
                AppTheme.Light => ThemeVariant.Light,
                AppTheme.Dark => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }
    }
}
