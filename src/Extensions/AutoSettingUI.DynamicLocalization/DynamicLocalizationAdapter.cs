using System.ComponentModel;
using AutoSettingUI.Core.Interfaces;
using DynamicLocalization.Core;

namespace AutoSettingUI.DynamicLocalization;

/// <summary>Adapts a DynamicLocalization culture service without owning it.</summary>
public sealed class DynamicLocalizationAdapter : ILocalizationService, IDisposable
{
    private readonly ICultureService _service;

    public DynamicLocalizationAdapter(ICultureService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _service.CultureChanged += OnCultureChanged;
        _service.PropertyChanged += OnPropertyChanged;
    }

    public event EventHandler? LocalizationChanged;

    public string? GetString(string key) => _service.GetString(key);

    public void Dispose()
    {
        _service.CultureChanged -= OnCultureChanged;
        _service.PropertyChanged -= OnPropertyChanged;
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e) =>
        LocalizationChanged?.Invoke(this, EventArgs.Empty);

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "Item[]")
            LocalizationChanged?.Invoke(this, EventArgs.Empty);
    }
}
