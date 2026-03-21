namespace AutoSettingUI.Ursa.Demo.Resources;

/// <summary>
/// Provides access to localized string resources for the Ursa demo application.
/// This class exposes a ResourceManager that loads strings from .resx files.
/// </summary>
/// <remarks>
/// Resource files:
/// - Strings.resx: Default (English) resources
/// - Strings.zh-CN.resx: Chinese (Simplified) resources
/// 
/// Usage with ResxLocalizationService:
/// <code>
/// var localizationService = new ResxLocalizationService(Strings.ResourceManager);
/// string localizedText = localizationService["Settings.Title"];
/// </code>
/// </remarks>
public static class Strings
{
    private static System.Resources.ResourceManager? _resourceManager;

    /// <summary>
    /// Gets the ResourceManager for accessing localized strings.
    /// The manager is lazily initialized on first access.
    /// </summary>
    public static System.Resources.ResourceManager ResourceManager
    {
        get
        {
            if (_resourceManager == null)
            {
                _resourceManager = new System.Resources.ResourceManager(
                    "AutoSettingUI.Ursa.Demo.Resources.Strings", 
                    typeof(Strings).Assembly);
            }
            return _resourceManager;
        }
    }
}
