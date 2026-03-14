namespace AutoSettingUI.Core.Interfaces;

/// <summary>
/// Provides access to property values on a target object.
/// </summary>
public interface IPropertyValueAccessor
{
    /// <summary>
    /// Gets the value of a property.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value.</returns>
    object? GetValue(object target, string propertyName);

    /// <summary>
    /// Sets the value of a property.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="value">The value to set.</param>
    void SetValue(object target, string propertyName, object? value);
}
