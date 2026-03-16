using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.WPF.Attributes;
using AutoSettingUI.Wpf.Demo.Controls;
using ReadOnlyAttribute = AutoSettingUI.Core.Attributes.ReadOnlyAttribute;
using DescriptionAttribute = AutoSettingUI.Core.Attributes.DescriptionAttribute;

namespace AutoSettingUI.Wpf.Demo.Models;

/// <summary>
/// Sample application settings class demonstrating various AutoSettingUI features.
/// </summary>
[SettingUI]
[MainHeader("Application Settings")]
public class ApplicationSettings
{
    [Title("Application Name")]
    public string AppName { get; set; } = "My Application";

    [Title("Version")]
    [ReadOnly(true)]
    public string Version { get; set; } = "1.0.0";

    [Title("Enable Logging")]
    public bool EnableLogging { get; set; } = true;

    [Title("Log Level")]
    public LogLevel CurLogLevel { get; set; } = LogLevel.Info;

    [Title("Max Log Size (MB)")]
    [Range(1, 100)]
    public int MaxLogSize { get; set; } = 10;

    [Title("Volume")]
    [ControlBinding(typeof(Slider), BindingProperty = "Value", FactoryMethod = nameof(VolumeFactory))]
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

/// <summary>
/// Sample user preferences class demonstrating delegate properties and collection editors.
/// WPF's CommandManager automatically handles CanExecute re-evaluation on UI changes.
/// </summary>
[SettingUI]
[MainHeader("User Preferences")]
public class UserPreferences : INotifyPropertyChanged
{
    private bool _isAdmin = true;
    private string _email = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [SubHeader("Display")]
    [Title("Theme")]
    public Theme CurTheme { get; set; } = Theme.Light;

    [Title("Language")] public string Language { get; set; } = "English";

    [Title("Font Size")] [Range(8, 32)] public int FontSize { get; set; } = 14;

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

    [Title("Notification Sound")] public bool NotificationSound { get; set; } = true;

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
    [Layout(Width = 200, Height = 28, Margin = "0,2,0,2")]
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
    public List<string> Tags { get; set; } = ["Important", "Work"];

    [Title("Versions (Read-Only Collection)")]
    [CollectionEditor(AllowAdd = false, AllowRemove = false, AllowReorder = false)]
    public List<string> Versions { get; set; } = ["1.0.0", "1.1.0", "2.0.0"];

    [Title("People (Default Editor)")]
    public List<Person> People1 { get; set; } =
    [
        new Person { Name = "John Doe", Age = 30, Email = "john@example.com" },
        new Person { Name = "Jane Smith", Age = 25, Email = "jane@example.com" }
    ];
    
    [Title("People (Custom Editor)")]
    [CollectionEditor(typeof(PersonCollectionEditor))]
    public List<Person> People { get; set; } =
    [
        new Person { Name = "John Doe", Age = 30, Email = "john@example.com" },
        new Person { Name = "Jane Smith", Age = 25, Email = "jane@example.com" }
    ];

    public UserPreferences()
    {
        ResetSettingsCommand = ResetSettings;
        ExportSettingsCommand = ExportSettings;
        ImportSettingsCommand = ImportSettings;
        AdminActionCommand = AdminAction;
    }

    private void ResetSettings()
    {
        CurTheme = Theme.Light;
        Language = "English";
        FontSize = 14;
        EnableNotifications = true;
        NotificationSound = true;
        System.Windows.MessageBox.Show("Settings have been reset!");
    }

    private void ExportSettings()
    {
        System.Windows.MessageBox.Show("Settings exported!");
    }

    private void ImportSettings()
    {
        System.Windows.MessageBox.Show("Settings imported!");
    }

    private void AdminAction()
    {
        System.Windows.MessageBox.Show("Admin action executed!");
    }

    public bool CanExport() => EnableNotifications;
    public bool CanImport() => !string.IsNullOrEmpty(Email);
    public bool CanEdit() => !IsAdmin;


}

/// <summary>
/// Sample network settings class.
/// </summary>
[SettingUI]
[MainHeader("Network Configuration")]
public class NetworkSettings
{
    [SubHeader("Connection")]
    [Title("Server Address")]
    public string ServerAddress { get; set; } = "localhost";

    [Title("Port")]
    [Range(1, 65535)]
    public int Port { get; set; } = 8080;

    [Title("Use HTTPS")]
    public bool UseHttps { get; set; } = false;

    [SubHeader("Authentication")]
    [Title("Username")]
    public string Username { get; set; } = "";

    [Title("Timeout (seconds)")]
    [Range(1, 300)]
    public int Timeout { get; set; } = 30;

    [SubHeader("Actions")]
    [Title("Test Connection")]
    [CommandCanExecute(nameof(CanTestConnection))]
    public Action? TestConnectionCommand { get; set; }

    [Title("Ping Server")]
    public Action? PingServerCommand { get; set; }

    public NetworkSettings()
    {
        TestConnectionCommand = TestConnection;
        PingServerCommand = PingServer;
    }

    private void TestConnection()
    {
        System.Windows.MessageBox.Show($"Testing connection to {ServerAddress}:{Port}...");
    }

    private void PingServer()
    {
        System.Windows.MessageBox.Show($"Pinging {ServerAddress}...");
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
/// Sample class demonstrating extended controls in WPF.
/// </summary>
[SettingUI]
[MainHeader("Extended UI Controls")]
public class ExtendedControlsSettings
{
    [Title("Background Color")]
    
    public Color ThemeColor { get; set; } = Colors.DodgerBlue;

    [Title("Release Date")]
    [DatePicker]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [Title("Toggle Feature")]
    [CheckBox]
    public bool EnableAdvancedFeature { get; set; } = true;

    [Title("Count")]
    [Range(0, 100)]
    public int ItemCount { get; set; } = 42;
}

/// <summary>
/// Settings class for theme management within the form for WPF.
/// </summary>
[SettingUI]
[MainHeader("Theme Personalization")]
public class ThemeSettings : INotifyPropertyChanged    
{
    private string _selectedTheme = "Light";

    public event PropertyChangedEventHandler? PropertyChanged;

    [Title("Application Theme")]
    [Description("Change the look and feel of the application")]
    [ItemsSource(typeof(ThemeSettings), nameof(AvailableThemes))]
    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (_selectedTheme != value)
            {
                _selectedTheme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTheme)));
            }
        }
    }

    public static List<string> AvailableThemes => new()
    {
        "Light",
        "Dark",
        "Blue",
        "High Contrast"
    };
}
