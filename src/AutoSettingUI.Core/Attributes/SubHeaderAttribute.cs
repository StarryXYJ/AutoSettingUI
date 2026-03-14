namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Marks a property as the start of a subsection in the settings form.
/// Sub headers group subsequent properties until the next SubHeader, MainHeader, or end.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SubHeaderAttribute : Attribute
{
    /// <summary>
    /// Gets the title of the sub header.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets or sets the icon identifier for the header (framework-specific).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubHeaderAttribute"/> class.
    /// </summary>
    /// <param name="title">The title displayed for this subsection.</param>
    public SubHeaderAttribute(string title)
    {
        Title = title;
    }
}
