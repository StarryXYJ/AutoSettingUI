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
    /// Gets or sets whether the name is a resource key for localization.
    /// When true, the Name value will be used as a key to lookup the localized string.
    /// </summary>
    public bool UseResourceKey { get; set; }
    
    /// <summary>
    /// Gets or sets whether the description is a resource key for localization.
    /// When true, the Description value will be used as a key to lookup the localized string.
    /// </summary>
    public bool UseDescriptionKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleAttribute"/> class.
    /// </summary>
    /// <param name="name">The display name for the property.</param>
    public TitleAttribute(string name)
    {
        Name = name;
    }
}
