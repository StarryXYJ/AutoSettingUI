using System.ComponentModel;

namespace AutoSettingUI.Core.Interfaces;

/// <summary>
/// Provides localization services for dynamic language switching in settings panels.
/// Implementations should support retrieving localized strings and notifying subscribers
/// when the current culture changes.
/// </summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the localized string for the specified resource key.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    string GetString(string key);

    /// <summary>
    /// Gets the localized string for the specified resource key using indexer syntax.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself if not found.</returns>
    string this[string key] => GetString(key);

    /// <summary>
    /// Gets the name of the current culture (e.g., "en", "zh-CN").
    /// </summary>
    string CurrentCulture { get; }

    /// <summary>
    /// Changes the current culture and raises the <see cref="CultureChanged"/> event.
    /// </summary>
    /// <param name="cultureName">The name of the culture to switch to (e.g., "en", "zh-CN").</param>
    void SetCulture(string cultureName);

    /// <summary>
    /// Occurs when the current culture is changed.
    /// UI panels subscribe to this event to update their displayed text.
    /// </summary>
    event EventHandler<CultureChangedEventArgs>? CultureChanged;
}

/// <summary>
/// Provides event data for the <see cref="ILocalizationService.CultureChanged"/> event.
/// </summary>
public class CultureChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the previous culture.
    /// </summary>
    public string OldCulture { get; }

    /// <summary>
    /// Gets the name of the new culture.
    /// </summary>
    public string NewCulture { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CultureChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldCulture">The previous culture name.</param>
    /// <param name="newCulture">The new culture name.</param>
    public CultureChangedEventArgs(string oldCulture, string newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }
}
