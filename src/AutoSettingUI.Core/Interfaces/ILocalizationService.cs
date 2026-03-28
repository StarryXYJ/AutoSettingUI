using System.ComponentModel;
using DynamicLocalization.Core;

namespace AutoSettingUI.Core.Interfaces;

/// <summary>
/// Provides localization services for dynamic language switching in settings panels.
/// This interface extends <see cref="ICultureService"/> from DynamicLocalization.Core
/// to provide full compatibility with the DynamicLocalization ecosystem.
/// </summary>
/// <remarks>
/// Implementations should support retrieving localized strings and notifying subscribers
/// when the current culture changes. You can use any ICultureService implementation
/// from DynamicLocalization.Core (JsonLocalizationProvider, ResxLocalizationProvider, etc.)
/// or create custom implementations.
/// </remarks>
public interface ILocalizationService : ICultureService, INotifyPropertyChanged
{
}
