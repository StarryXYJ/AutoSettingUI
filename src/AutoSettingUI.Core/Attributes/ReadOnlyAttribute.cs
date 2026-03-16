namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Marks a property as read-only. Can specify a boolean value directly or a method name
/// that returns a boolean to dynamically determine the read-only state.
/// For delegate properties (Action, Func), this controls the button's enabled state.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ReadOnlyAttribute : Attribute
{
    /// <summary>
    /// Gets whether the property is read-only (static value).
    /// If a method name is specified, this value is ignored.
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets the name of the method that returns a boolean indicating whether the property is read-only.
    /// The method must be parameterless and return bool.
    /// </summary>
    public string? MethodName { get; }

    /// <summary>
    /// Gets a value indicating whether a dynamic method is specified for determining read-only state.
    /// </summary>
    public bool IsDynamic => !string.IsNullOrEmpty(MethodName);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyAttribute"/> class with a static boolean value.
    /// </summary>
    /// <param name="isReadOnly">Whether the property is read-only.</param>
    public ReadOnlyAttribute(bool isReadOnly)
    {
        IsReadOnly = isReadOnly;
        MethodName = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyAttribute"/> class with a method name.
    /// </summary>
    /// <param name="methodName">The name of the method to call for dynamic read-only evaluation.</param>
    public ReadOnlyAttribute(string methodName)
    {
        MethodName = methodName;
        IsReadOnly = false; // Default, will be determined by method at runtime
    }
}
