namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Marks a class to be automatically rendered as a settings UI form.
/// This is the primary attribute that enables AutoSettingUI generation for a class.
/// Rendering order follows the order of items in the Targets collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class SettingUIAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the icon identifier for this setting class (framework-specific).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the default control factory type for this setting class.
    /// This factory will be used for all properties unless overridden by [ControlBinding].
    /// </summary>
    public Type? ControlFactory { get; set; }

    /// <summary>
    /// Gets or sets the default factory method name to create controls.
    /// This method should be static and return a control instance.
    /// </summary>
    public string? FactoryMethod { get; set; }
}
