using System.Collections;
using System.Reflection;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.Extension.Shared;

/// <summary>
/// Base class for control factories providing shared functionality across WPF, Avalonia, and Ursa.
/// </summary>
public abstract class ControlFactoryBase
{
    protected readonly SettingClassDescriptor? _classDescriptor;
    protected readonly IPropertyValueAccessor? _accessor;

    /// <summary>
    /// Event raised when a control with dynamic read-only or CanExecute state is created.
    /// </summary>
    public event Action<object, Func<bool>>? ReadOnlyControlCreated;

    protected ControlFactoryBase()
    {
    }

    protected ControlFactoryBase(SettingClassDescriptor? classDescriptor, IPropertyValueAccessor? accessor)
    {
        _classDescriptor = classDescriptor;
        _accessor = accessor;
    }

    /// <summary>
    /// Gets the value of a property from the target object.
    /// </summary>
    protected object? GetValue(object target, string propertyName)
    {
        if (_accessor != null)
        {
            return _accessor.GetValue(target, propertyName);
        }

        var prop = target.GetType().GetProperty(propertyName);
        return prop?.GetValue(target);
    }

    /// <summary>
    /// Sets the value of a property on the target object.
    /// </summary>
    protected void SetValue(object target, string propertyName, object? value)
    {
        if (_accessor != null)
        {
            _accessor.SetValue(target, propertyName, value);
            return;
        }

        var prop = target.GetType().GetProperty(propertyName);
        if (prop is not null && prop.CanWrite)
        {
            prop.SetValue(target, value);
        }
    }

    /// <summary>
    /// Determines if a property is effectively read-only.
    /// </summary>
    protected bool IsEffectivelyReadOnly(PropertyDescriptor prop, object target)
    {
        // If dynamic method is specified, call that
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            var method = target.GetType().GetMethod(prop.ReadOnlyMethodName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }
        }

        return prop.IsReadOnly;
    }

    /// <summary>
    /// Determines if a delegate command can execute.
    /// </summary>
    protected bool CanExecuteDelegate(PropertyDescriptor prop, object target)
    {
        // If CanExecuteMethodName is specified, call that method
        if (!string.IsNullOrEmpty(prop.CanExecuteMethodName))
        {
            var method = target.GetType().GetMethod(prop.CanExecuteMethodName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }
        }

        // If ReadOnly is specified, use that
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            return !IsEffectivelyReadOnly(prop, target);
        }

        // Default: always can execute
        return !prop.IsReadOnly;
    }

    /// <summary>
    /// Raises the ReadOnlyControlCreated event.
    /// </summary>
    protected void OnReadOnlyControlCreated(object control, Func<bool> isReadOnlyGetter)
    {
        ReadOnlyControlCreated?.Invoke(control, isReadOnlyGetter);
    }

    /// <summary>
    /// Gets the PropertyInfo for a property from the target object.
    /// </summary>
    protected PropertyInfo? GetPropertyInfo(object target, string propertyName)
    {
        return target.GetType().GetProperty(propertyName, 
            BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets a type from its name, searching in all loaded assemblies.
    /// </summary>
    protected Type? GetTypeFromName(string typeName)
    {
        // Try direct type resolution first
        var type = Type.GetType(typeName);
        if (type is not null)
            return type;

        // Search in all loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type is not null)
                return type;
        }

        return null;
    }

    /// <summary>
    /// Converts a double value to the specified type.
    /// </summary>
    protected object ConvertValue(double value, string typeName)
    {
        var lowerType = typeName.ToLowerInvariant();

        if (lowerType.Contains("int32") || lowerType == "int")
            return (int)value;
        if (lowerType.Contains("single") || lowerType == "float")
            return (float)value;
        if (lowerType.Contains("double") || lowerType == "double")
            return value;

        return value;
    }

    /// <summary>
    /// Determines if a delegate is async (returns Task).
    /// </summary>
    protected bool IsAsyncDelegate(Delegate? delegateValue)
    {
        if (delegateValue == null) return false;

        var method = delegateValue.Method;
        return method.ReturnType == typeof(Task) ||
               method.ReturnType == typeof(ValueTask) ||
               (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
