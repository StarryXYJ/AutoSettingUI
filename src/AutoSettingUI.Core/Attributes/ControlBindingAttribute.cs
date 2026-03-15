namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a custom control type to use for a property.
/// This allows overriding the default control selection with a specific control.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ControlBindingAttribute : ControlBindingAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    public ControlBindingAttribute(Type controlType)
    {
        ControlType = controlType;
    }
}
