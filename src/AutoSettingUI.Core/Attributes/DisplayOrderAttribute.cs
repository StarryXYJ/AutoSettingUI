namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies the display order of a property in the settings UI.
/// Properties with lower order values are displayed first.
/// Properties with the same order value are displayed in their declaration order.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DisplayOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the display order value.
    /// Lower values are displayed first.
    /// Default is 0.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DisplayOrderAttribute"/> class.
    /// </summary>
    /// <param name="order">The display order value. Lower values are displayed first.</param>
    public DisplayOrderAttribute(int order)
    {
        Order = order;
    }
}
