namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a description or tooltip text for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class.
    /// </summary>
    /// <param name="text">The description text.</param>
    public DescriptionAttribute(string text)
    {
        Text = text;
    }
}
