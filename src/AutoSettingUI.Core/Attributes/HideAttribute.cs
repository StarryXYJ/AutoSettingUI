namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Hides a property from the settings form. Can specify a boolean value directly or a method name
/// that returns a boolean to dynamically determine the visibility.
/// When the method/property returns true, the property is hidden; when false, it is visible.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class HideAttribute : Attribute
{
    /// <summary>
    /// Gets whether the property is hidden (static value).
    /// If a method name is specified, this value is ignored.
    /// </summary>
    public bool IsHidden { get; }

    /// <summary>
    /// Gets the name of the method or property that returns a boolean indicating whether the property is hidden.
    /// The method must be parameterless and return bool.
    /// The property must be a boolean property.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets a value indicating whether a dynamic method is specified for determining visibility.
    /// </summary>
    public bool IsDynamic => !string.IsNullOrEmpty(MethodName);

    /// <summary>
    /// Initializes a new instance of the <see cref="HideAttribute"/> class with a static boolean value.
    /// </summary>
    /// <param name="isHidden">Whether the property is hidden.</param>
    public HideAttribute(bool isHidden)
    {
        IsHidden = isHidden;
        MethodName = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HideAttribute"/> class with a method name.
    /// </summary>
    /// <param name="methodName">The name of the method or property to call for dynamic visibility evaluation.</param>
    public HideAttribute(string methodName)
    {
        MethodName = methodName;
        IsHidden = false; // Default, will be determined by method at runtime
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HideAttribute"/> class that hides the property.
    /// </summary>
    public HideAttribute()
    {
        IsHidden = true;
        MethodName = null;
    }
}
