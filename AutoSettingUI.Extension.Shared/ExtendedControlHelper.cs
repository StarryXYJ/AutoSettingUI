using System.Reflection;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.Extension.Shared;

/// <summary>
/// Shared helper for creating extended controls from ControlBindingAttribute.
/// Used by Avalonia and Ursa implementations.
/// </summary>
public static class ExtendedControlHelper
{
    /// <summary>
    /// Creates a control instance from a ControlBindingAttribute.
    /// </summary>
    /// <param name="attr">The control binding attribute.</param>
    /// <param name="propertyType">The type of the property being bound.</param>
    /// <returns>The created control, or null if creation fails.</returns>
    public static object? CreateControl(ControlBindingAttribute attr, Type propertyType)
    {
        if (attr.ControlType == null)
            return null;

        var controlType = attr.ControlType;
        object? control = null;

        if (!string.IsNullOrEmpty(attr.FactoryMethod))
        {
            control = TryInvokeFactoryMethod(attr, controlType, propertyType);
        }

        control ??= Activator.CreateInstance(controlType);

        return control;
    }

    /// <summary>
    /// Gets the AvaloniaProperty for binding from a control type.
    /// </summary>
    /// <param name="controlType">The control type.</param>
    /// <param name="bindingPropertyName">The name of the binding property (without "Property" suffix).</param>
    /// <returns>The AvaloniaProperty field info, or null if not found.</returns>
    public static FieldInfo? GetBindingPropertyField(Type controlType, string bindingPropertyName)
    {
        if (string.IsNullOrEmpty(bindingPropertyName))
            return null;

        return controlType.GetField(
            bindingPropertyName + "Property",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Checks if a property has a ControlBindingAttribute.
    /// </summary>
    public static bool HasControlBindingAttribute(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<ControlBindingAttribute>() != null;
    }

    /// <summary>
    /// Gets the ControlBindingAttribute from a property.
    /// </summary>
    public static ControlBindingAttribute? GetControlBindingAttribute(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute<ControlBindingAttribute>();
    }

    private static object? TryInvokeFactoryMethod(ControlBindingAttribute attr, Type controlType, Type propertyType)
    {
        var method = controlType.GetMethod(attr.FactoryMethod!,
            BindingFlags.Static | BindingFlags.Public);

        if (method != null)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
                return method.Invoke(null, null);
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Type))
                return method.Invoke(null, new object[] { propertyType });
        }

        var attrMethod = attr.GetType().GetMethod(attr.FactoryMethod!,
            BindingFlags.Instance | BindingFlags.Public);

        if (attrMethod != null)
        {
            var parameters = attrMethod.GetParameters();
            if (parameters.Length == 0)
                return attrMethod.Invoke(attr, null);
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Type))
                return attrMethod.Invoke(attr, new object[] { propertyType });
        }

        return null;
    }
}
