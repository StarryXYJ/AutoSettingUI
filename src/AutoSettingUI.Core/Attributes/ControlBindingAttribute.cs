namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a custom control type to use for a property.
/// This allows overriding the default control selection with a specific control.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class ControlBindingAttribute:Attribute
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
    /// If not specified and a factory method is used, the factory is responsible for setting up bindings.
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    /// <param name="bindingProperty">The property of the control to bind to.</param>
    public ControlBindingAttribute(Type controlType, string bindingProperty)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlBindingAttribute"/> class.
    /// </summary>
    /// <param name="controlType">The type of the custom control to use.</param>
    /// <param name="bindingProperty">The property of the control to bind to.</param>
    /// <param name="factoryMethod">Factory Method.</param>
    public ControlBindingAttribute(Type controlType, string bindingProperty, string factoryMethod)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
        FactoryMethod = factoryMethod;
    }
}
