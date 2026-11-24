using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.WPF.Attributes;
using AutoSettingUI.Wpf.Demo.Controls;
using ReadOnlyAttribute = AutoSettingUI.Core.Attributes.ReadOnlyAttribute;
using DescriptionAttribute = AutoSettingUI.Core.Attributes.DescriptionAttribute;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSettingUI.Wpf.Demo.Models;

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
    [Hide(nameof(ShouldHideLogging))]
    public LogLevel CurLogLevel { get; set; } = LogLevel.Info;

    [Title("Settings.MaxLogSize", UseResourceKey = true)]
    [Range(1, 100)]
    [Hide(nameof(ShouldHideLogging))]
    public int MaxLogSize { get; set; } = 10;

    [Title("Settings.Volume", UseResourceKey = true)]
    [ControlBinding(typeof(Slider), BindingProperty = "Value", FactoryMethod = nameof(VolumeFactory))]
    public double Volume { get; set; } = 50.0;

    [Hide]
    public string InternalId { get; set; } = Guid.NewGuid().ToString();

    public bool ShouldHideLogging => !EnableLogging;

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
    private string _email = "";

    [SubHeader("Display")]
    [Title("Theme")]
    public Theme CurTheme { get; set; } = Theme.Light;

    [Title("Language")]
    public string Language { get; set; } = "English";

    [Title("Font Size")]
    [Range(8, 32)]
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
    private string _readOnlyField = "This field is read-only";

    [Title("Dynamic ReadOnly")]
    [ReadOnly(nameof(CanEdit))]
    [ObservableProperty]
    private string _dynamicReadOnlyField = "Only editable by admins";

    [Title("Dynamic ReadOnly")]
    [ReadOnly(nameof(CanEdit))]
    [ObservableProperty]
    [Range(0, 100)]
    private int _dynamicReadOnlyFieldInt = 42;

    [ControlBinding(typeof(System.Windows.Controls.CheckBox), "IsChecked")]
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
    public List<string> Tags { get; set; } = ["Important", "Work"];

    [Title("Selected Tag")]
    [Description("The currently selected tag from the list above")]
    [ObservableProperty]
    private string? _selectedTag;

    [Title("Versions (Read-Only Collection)")]
    [CollectionEditor(AllowAdd = false, AllowRemove = false, AllowReorder = false)]
    public List<string> Versions { get; set; } = ["1.0.0", "1.1.0", "2.0.0"];

    [Title("People (Default Editor)")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedPerson1))]
    public List<Person> People1 { get; set; } =
    [
        new Person { Name = "John Doe", Age = 30, Email = "john@example.com" },
        new Person { Name = "Jane Smith", Age = 25, Email = "jane@example.com" }
    ];

    [Title("Selected Person (Default Editor)")]
    [Description("The currently selected person from the default editor above")]
    [ObservableProperty]
    private Person? _selectedPerson1;

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
        System.Windows.MessageBox.Show($"Testing connection to {ServerAddress}:{Port}...");
    }

    private void PingServer()
    {
        System.Windows.MessageBox.Show($"Pinging {ServerAddress}...");
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
    public Color ThemeColor { get; set; } = Colors.DodgerBlue;

    [Title("Settings.ReleaseDate", UseResourceKey = true)]
    [DatePicker]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [Title("Toggle Feature")]
    [CheckBox]
    public bool EnableAdvancedFeature { get; set; } = true;

    [Title("Settings.ItemCount", UseResourceKey = true)]
    [Range(0, 100)]
    [ObservableProperty]
    [DisplayOrder(1)]
    private int _itemCount = 42;

    #region ControlBinding Examples

    [SubHeader("ControlBinding Examples")]

    [Title("Progress (Auto-bind)")]
    [Description("Uses ControlBinding with BindingProperty - auto binds to Value")]
    [ControlBinding(typeof(System.Windows.Controls.ProgressBar), "Value")]
    public double ProgressValue { get; set; } = 50.0;

    [Title("Custom Slider (Factory Method)")]
    [Description("Factory method handles binding manually - can customize converter, etc.")]
    [ControlBinding(typeof(System.Windows.Controls.Slider), FactoryMethod = nameof(CreateCustomSlider))]
    public double CustomSliderValue { get; set; } = 75.0;

    public System.Windows.Controls.Slider CreateCustomSlider()
    {
        var slider = new System.Windows.Controls.Slider
        {
            Minimum = 0,
            Maximum = 100,
            Width = 200
        };

        slider.SetBinding(System.Windows.Controls.Slider.ValueProperty,
            new System.Windows.Data.Binding(nameof(CustomSliderValue))
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.TwoWay
            });

        return slider;
    }

    [Title("Auto-detected TextBox")]
    [Description("Only control type specified - auto-detects Text property")]
    [ControlBinding(typeof(System.Windows.Controls.TextBox))]
    public string AutoDetectedText { get; set; } = "Auto-detected binding";

    [Title("Rating (Complex Factory)")]
    [Description("Factory creates a custom rating control with full customization")]
    [ControlBinding(typeof(System.Windows.Controls.StackPanel), FactoryMethod = nameof(CreateRatingControl))]
    [ObservableProperty]
    private int _rating = 3;

    public System.Windows.Controls.StackPanel CreateRatingControl()
    {
        var panel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal
        };

        for (int i = 1; i <= 5; i++)
        {
            var starIndex = i;
            var button = new System.Windows.Controls.Button
            {
                Content = "★",
                FontSize = 24,
                Width = 40,
                Height = 40
            };

            button.Click += (s, e) => Rating = starIndex;

            button.SetBinding(System.Windows.Controls.Button.ForegroundProperty,
                new System.Windows.Data.Binding(nameof(Rating))
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

public enum AppTheme
{
    Default,
    Light,
    Dark,
    Blue,
    HighContrast
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
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public static List<AppTheme> AvailableThemes => new()
    {
        AppTheme.Default,
        AppTheme.Light,
        AppTheme.Dark,
        AppTheme.Blue,
        AppTheme.HighContrast
    };
}

public class RatingConverter : System.Windows.Data.IValueConverter
{
    private readonly int _starIndex;

    public RatingConverter(int starIndex)
    {
        _starIndex = starIndex;
    }

    public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int rating)
        {
            return rating >= _starIndex
                ? Brushes.Gold
                : Brushes.Gray;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new System.NotImplementedException();
    }
}
