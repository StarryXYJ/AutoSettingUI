using System.Windows.Controls;
using System.Windows.Media;
using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Wpf.Demo.Models;

/// <summary>
/// Sample application settings class demonstrating various AutoSettingUI features.
/// </summary>
[SettingUI(Category = "Application", Order = 1)]
[MainHeader("Application Settings")]
public class ApplicationSettings
{
    [Title("Application Name")]
    public string AppName { get; set; } = "My Application";

    
    public string Version { get; set; } = "1.0.0";

    [Title("Enable Logging")]
    public bool EnableLogging { get; set; } = true;

    [Title("Log Level")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    [Title("Max Log Size (MB)")]
    [Range(1, 100)]
    public int MaxLogSize { get; set; } = 10;

    [Title("Volume")]
    [ControlBinding(typeof(Slider), BindingProperty = "Value",FactoryMethod = nameof(VolumeFactory))]
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
/// Sample user preferences class.
/// </summary>
[SettingUI(Category = "User", Order = 2)]
[MainHeader("User Preferences")]
public class UserPreferences
{
    [SubHeader("Display")]
    [Title("Theme")]
    public Theme Theme { get; set; } = Theme.Light;

    [Title("Language")]
    public string Language { get; set; } = "English";

    [Title("Font Size")]
    [Range(8, 32)]
    public int FontSize { get; set; } = 14;

    [SubHeader("Notifications")]
    [Title("Enable Notifications")]
    public bool EnableNotifications { get; set; } = true;

    [Title("Notification Sound")]
    public bool NotificationSound { get; set; } = true;

    [Title("Email Address")]
    public string Email { get; set; } = "";
}

/// <summary>
/// Sample network settings class.
/// </summary>
[SettingUI(Category = "Network", Order = 3)]
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

    [Title("Password")]
    public string Password { get; set; } = "";

    [Title("Timeout (seconds)")]
    [Range(1, 300)]
    public int Timeout { get; set; } = 30;
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
