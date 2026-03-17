namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Declares default control binding metadata for attributes derived from <see cref="ControlBindingAttribute"/>.
/// This is used by the source generator for AOT-safe control binding without reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ControlBindingDefaultsAttribute : Attribute
{
    /// <summary>
    /// Gets the control type to use.
    /// </summary>
    public Type ControlType { get; }

    /// <summary>
    /// Gets the target property on the control to bind to.
    /// </summary>
    public string BindingProperty { get; }

    /// <summary>
    /// Gets the factory method name (optional).
    /// </summary>
    public string? FactoryMethod { get; }

    public ControlBindingDefaultsAttribute(Type controlType, string bindingProperty)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
    }

    public ControlBindingDefaultsAttribute(Type controlType, string bindingProperty, string factoryMethod)
    {
        ControlType = controlType;
        BindingProperty = bindingProperty;
        FactoryMethod = factoryMethod;
    }
}
