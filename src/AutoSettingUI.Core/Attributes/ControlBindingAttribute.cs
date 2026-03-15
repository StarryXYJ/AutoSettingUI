namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a custom control type to use for a property.
/// This allows overriding the default control selection with a specific control.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ControlBindingAttribute : ControlBindingAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    public ControlBindingAttribute(Type controlType)
    {
        ControlType = controlType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    /// <param name="bindingProperty">The property of the control.</param>
    public ControlBindingAttribute(Type controlType, string bindingProperty)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    /// <param name="bindingProperty">The property of the control.</param>
    /// <param name="factoryMethod">Factory Method.</param>
    public ControlBindingAttribute(Type controlType, string bindingProperty, string factoryMethod)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
        FactoryMethod = factoryMethod;
    }
}
