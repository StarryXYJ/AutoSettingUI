using AutoSettingUI.Core.Interfaces;

namespace AutoSettingUI.Core.Registry;

/// <summary>
/// A global registry for AOT-compatible setting providers.
/// The source generator automatically registers itself here.
/// </summary>
public static class AotSettingRegistry
{
    private static ISettingDescriptorProvider? _provider;
    private static IPropertyValueAccessor? _accessor;

    /// <summary>
    /// Gets or sets the global AOT provider.
    /// </summary>
    public static ISettingDescriptorProvider? Provider 
    { 
        get => _provider; 
        set => _provider = value; 
    }

    /// <summary>
    /// Gets or sets the global AOT accessor.
    /// </summary>
    public static IPropertyValueAccessor? Accessor 
    { 
        get => _accessor; 
        set => _accessor = value; 
    }
}
