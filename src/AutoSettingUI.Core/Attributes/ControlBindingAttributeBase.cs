namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Base class for attributes that bind a property to a specific UI control.
/// </summary>
public abstract class ControlBindingAttributeBase : Attribute
{
    /// <summary>
    /// Gets the type of the custom control to use.
    /// </summary>
    public Type? ControlType { get; protected set; }

    /// <summary>
    /// Gets or sets additional parameters for control initialization.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the target property on the custom control to bind to.
    /// </summary>
    public string? BindingProperty { get; set; }

    /// <summary>
    /// Gets or sets a static factory method name on the ControlType to instantiate the control.
    /// </summary>
    public string? FactoryMethod { get; set; }
}
