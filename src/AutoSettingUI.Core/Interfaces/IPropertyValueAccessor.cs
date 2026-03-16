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

    /// <summary>
    /// Gets an enum value from its string representation.
    /// AOT-safe implementations should override this to provide static resolution.
    /// </summary>
    /// <param name="typeName">The full name of the enum type.</param>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The enum value object.</returns>
    object? GetEnumValue(string typeName, string value);
}
