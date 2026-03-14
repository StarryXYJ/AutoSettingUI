namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies the display name for a property in the settings form.
/// If not specified, the property name will be used.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class TitleAttribute : Attribute
{
    /// <summary>
    /// Gets the display name for the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the description/tooltip for the property.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleAttribute"/> class.
    /// </summary>
    /// <param name="name">The display name for the property.</param>
    public TitleAttribute(string name)
    {
        Name = name;
    }
}
