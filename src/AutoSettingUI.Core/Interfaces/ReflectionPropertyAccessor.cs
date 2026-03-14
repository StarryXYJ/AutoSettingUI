namespace AutoSettingUI.Core.Interfaces;

/// <summary>
/// Provides reflection-based access to property values on a target object.
/// This implementation uses <see cref="System.Reflection"/> and is suitable
/// for non-AOT runtime scenarios. For AOT-safe access, use the generated
/// <c>AutoSettingUI.Generated.GeneratedSettingProvider</c> instead.
/// </summary>
/// <example>
/// <code>
/// IPropertyValueAccessor accessor = new ReflectionPropertyAccessor();
/// var value = accessor.GetValue(mySettings, "Volume");
/// accessor.SetValue(mySettings, "Volume", 75);
/// </code>
/// </example>
public class ReflectionPropertyAccessor : IPropertyValueAccessor
{
    /// <inheritdoc />
    public object? GetValue(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName);
        return prop?.GetValue(target);
    }

    /// <inheritdoc />
    public void SetValue(object target, string propertyName, object? value)
    {
        var prop = target.GetType().GetProperty(propertyName);
        if (prop?.CanWrite == true)
            prop.SetValue(target, value);
    }
}
