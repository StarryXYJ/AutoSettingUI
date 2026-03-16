namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a placeholder text for input controls.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class PlaceholderAttribute : Attribute
{
    /// <summary>
    /// Gets the placeholder text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceholderAttribute"/> class.
    /// </summary>
    /// <param name="text">The placeholder text.</param>
    public PlaceholderAttribute(string text)
    {
        Text = text;
    }
}
