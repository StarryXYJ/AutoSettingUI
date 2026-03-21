using System.ComponentModel;
using System.Globalization;
using System.Resources;
using AutoSettingUI.Core.Interfaces;

namespace AutoSettingUI.Core.Services;

/// <summary>
/// A localization service that uses .NET .resx resource files for translations.
/// This is the recommended approach for applications that already use .resx files.
/// </summary>
/// <example>
/// // Create from a resource type
/// var service = new ResxLocalizationService(typeof(Strings));
/// 
/// // Or create from a ResourceManager
/// var service = new ResxLocalizationService(Strings.ResourceManager);
/// 
/// // Switch culture
/// service.SetCulture("zh-CN");
/// </example>
public class ResxLocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private CultureInfo _currentCulture;

    /// <summary>
    /// Gets the name of the current culture.
    /// </summary>
    public string CurrentCulture => _currentCulture.Name;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the current culture is changed.
    /// </summary>
    public event EventHandler<CultureChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Initializes a new instance using the specified ResourceManager.
    /// Uses the current UI culture as the default.
    /// </summary>
    /// <param name="resourceManager">The ResourceManager to retrieve strings from.</param>
    public ResxLocalizationService(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
        _currentCulture = CultureInfo.CurrentUICulture;
    }

    /// <summary>
    /// Initializes a new instance using the specified ResourceManager and default culture.
    /// </summary>
    /// <param name="resourceManager">The ResourceManager to retrieve strings from.</param>
    /// <param name="defaultCulture">The default culture name (e.g., "en", "zh-CN").</param>
    public ResxLocalizationService(ResourceManager resourceManager, string defaultCulture)
    {
        _resourceManager = resourceManager;
        _currentCulture = new CultureInfo(defaultCulture);
    }

    /// <summary>
    /// Initializes a new instance using the specified resource type.
    /// Uses the current UI culture as the default.
    /// </summary>
    /// <param name="resourceType">The type of the resource class (e.g., typeof(Strings)).</param>
    public ResxLocalizationService(Type resourceType)
        : this(new ResourceManager(resourceType))
    {
    }

    /// <summary>
    /// Initializes a new instance using the specified resource type and default culture.
    /// </summary>
    /// <param name="resourceType">The type of the resource class (e.g., typeof(Strings)).</param>
    /// <param name="defaultCulture">The default culture name (e.g., "en", "zh-CN").</param>
    public ResxLocalizationService(Type resourceType, string defaultCulture)
        : this(new ResourceManager(resourceType), defaultCulture)
    {
    }

    /// <summary>
    /// Gets the localized string for the specified key from the resource file.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return key;

        var value = _resourceManager.GetString(key, _currentCulture);
        return value ?? key;
    }

    /// <summary>
    /// Changes the current culture by name.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "en", "zh-CN").</param>
    public void SetCulture(string cultureName)
    {
        if (_currentCulture.Name == cultureName) return;

        var oldCulture = _currentCulture.Name;
        _currentCulture = new CultureInfo(cultureName);

        CultureChanged?.Invoke(this, new CultureChangedEventArgs(oldCulture, cultureName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    /// <summary>
    /// Changes the current culture using a CultureInfo object.
    /// </summary>
    /// <param name="culture">The culture to switch to.</param>
    public void SetCulture(CultureInfo culture)
    {
        if (_currentCulture.Equals(culture)) return;

        var oldCulture = _currentCulture.Name;
        _currentCulture = culture;

        CultureChanged?.Invoke(this, new CultureChangedEventArgs(oldCulture, culture.Name));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
