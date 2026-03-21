using System.Collections.Concurrent;
using System.ComponentModel;
using AutoSettingUI.Core.Interfaces;

namespace AutoSettingUI.Core.Services;

/// <summary>
/// A simple dictionary-based localization service for storing translations in memory.
/// Useful for applications that load translations from JSON, databases, or other sources.
/// </summary>
/// <example>
/// var service = new DictionaryLocalizationService("en");
/// 
/// // Register translations for a culture
/// service.RegisterCulture("en", new Dictionary&lt;string, string&gt;
/// {
///     ["Settings.Theme"] = "Theme",
///     ["Settings.Language"] = "Language"
/// });
/// 
/// service.RegisterCulture("zh-CN", new Dictionary&lt;string, string&gt;
/// {
///     ["Settings.Theme"] = "主题",
///     ["Settings.Language"] = "语言"
/// });
/// 
/// // Switch culture
/// service.SetCulture("zh-CN");
/// </example>
public class DictionaryLocalizationService : ILocalizationService
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cultures = new();
    private string _currentCulture = "en";

    /// <summary>
    /// Gets the name of the current culture.
    /// </summary>
    public string CurrentCulture => _currentCulture;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the current culture is changed.
    /// </summary>
    public event EventHandler<CultureChangedEventArgs>? CultureChanged;

    /// <summary>
    /// Initializes a new instance with "en" as the default culture.
    /// </summary>
    public DictionaryLocalizationService()
    {
        _cultures["en"] = new Dictionary<string, string>();
    }

    /// <summary>
    /// Initializes a new instance with the specified default culture.
    /// </summary>
    /// <param name="defaultCulture">The default culture name (e.g., "en", "zh-CN").</param>
    public DictionaryLocalizationService(string defaultCulture)
    {
        _currentCulture = defaultCulture;
        _cultures[_currentCulture] = new Dictionary<string, string>();
    }

    /// <summary>
    /// Registers all translations for a culture at once.
    /// Replaces any existing translations for that culture.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "en", "zh-CN").</param>
    /// <param name="translations">A dictionary of key-value pairs for translations.</param>
    public void RegisterCulture(string cultureName, Dictionary<string, string> translations)
    {
        _cultures[cultureName] = new Dictionary<string, string>(translations);
    }

    /// <summary>
    /// Adds or updates a single translation for a culture.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "en", "zh-CN").</param>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The translated string.</param>
    public void AddTranslation(string cultureName, string key, string value)
    {
        if (!_cultures.TryGetValue(cultureName, out var culture))
        {
            culture = new Dictionary<string, string>();
            _cultures[cultureName] = culture;
        }
        culture[key] = value;
    }

    /// <summary>
    /// Gets the localized string for the specified key.
    /// Falls back to English if the key is not found in the current culture,
    /// then returns the key itself if still not found.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    public string GetString(string key)
    {
        if (_cultures.TryGetValue(_currentCulture, out var culture) &&
            culture.TryGetValue(key, out var value))
        {
            return value;
        }

        if (_cultures.TryGetValue("en", out var defaultCulture) &&
            defaultCulture.TryGetValue(key, out var defaultValue))
        {
            return defaultValue;
        }

        return key;
    }

    /// <summary>
    /// Changes the current culture.
    /// </summary>
    /// <param name="cultureName">The culture name to switch to (e.g., "en", "zh-CN").</param>
    public void SetCulture(string cultureName)
    {
        if (_currentCulture == cultureName) return;

        var oldCulture = _currentCulture;
        _currentCulture = cultureName;

        CultureChanged?.Invoke(this, new CultureChangedEventArgs(oldCulture, cultureName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    /// <summary>
    /// Checks if translations are registered for a specific culture.
    /// </summary>
    /// <param name="cultureName">The culture name to check.</param>
    /// <returns>True if translations exist for the culture; otherwise, false.</returns>
    public bool HasCulture(string cultureName)
    {
        return _cultures.ContainsKey(cultureName);
    }
}
