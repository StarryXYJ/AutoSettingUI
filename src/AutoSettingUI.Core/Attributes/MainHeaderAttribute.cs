namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Marks a property as the start of a main section in the settings form.
/// Main headers create a container that groups subsequent properties until the next MainHeader or end.
/// They also appear in the navigation table of contents.
/// </summary>
[AttributeUsage(AttributeTargets.Class /*| AttributeTargets.Property | AttributeTargets.Field*/, Inherited = false, AllowMultiple = false)]
public sealed class MainHeaderAttribute : Attribute
{
    /// <summary>
    /// Gets the title of the main header.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the icon identifier for the header (framework-specific).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the display order of this header.
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Gets or sets whether the title is a resource key for localization.
    /// When true, the Title value will be used as a key to lookup the localized string.
    /// </summary>
    public bool UseResourceKey { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainHeaderAttribute"/> class.
    /// </summary>
    /// <param name="title">The title displayed for this section.</param>
    public MainHeaderAttribute(string title)
    {
        Title = title;
    }
}
