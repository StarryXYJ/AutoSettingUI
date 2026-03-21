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
    /// Gets or sets whether the text is a resource key for localization.
    /// When true, the Text value will be used as a key to lookup the localized string.
    /// </summary>
    public bool UseResourceKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class.
    /// </summary>
    /// <param name="text">The description text.</param>
    public DescriptionAttribute(string text)
    {
        Text = text;
    }
}
