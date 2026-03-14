namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a custom control type to use for a property.
/// This allows overriding the default control selection with a specific control.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ControlBindingAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the custom control to use.
    /// </summary>
    public Type ControlType { get; }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    public ControlBindingAttribute(Type controlType)
    {
        ControlType = controlType;
    }
}
