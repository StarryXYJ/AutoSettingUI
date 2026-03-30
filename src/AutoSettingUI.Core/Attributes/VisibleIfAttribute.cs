namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Conditionally shows or hides a property based on a method's return value.
/// The method must be parameterless and return bool.
/// When the method returns true, the property is visible; when false, it is hidden.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class VisibleIfAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the method that returns a boolean indicating whether the property should be visible.
    /// The method must be parameterless and return bool.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VisibleIfAttribute"/> class.
    /// </summary>
    /// <param name="methodName">The name of the method to call for dynamic visibility evaluation.</param>
    public VisibleIfAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
