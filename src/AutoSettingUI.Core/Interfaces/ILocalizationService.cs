namespace AutoSettingUI.Core.Interfaces;

/// <summary>Provides localized strings to a settings panel.</summary>
public interface ILocalizationService
{
    /// <summary>Returns a translation, or null when the key is unavailable.</summary>
    string? GetString(string key);

    /// <summary>Raised when translations may have changed, including a language switch.</summary>
    /// <remarks>Panels marshal notifications to their UI thread.</remarks>
    event EventHandler? LocalizationChanged;
}
