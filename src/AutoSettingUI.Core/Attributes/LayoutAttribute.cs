namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies layout properties for a control (width, height, etc.).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class LayoutAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the width of the control.
    /// Use double.NaN for Auto width.
    /// </summary>
    public double Width { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the height of the control.
    /// Use double.NaN for Auto height.
    /// </summary>
    public double Height { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the minimum width of the control.
    /// </summary>
    public double MinWidth { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the minimum height of the control.
    /// </summary>
    public double MinHeight { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the maximum width of the control.
    /// </summary>
    public double MaxWidth { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the maximum height of the control.
    /// </summary>
    public double MaxHeight { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the horizontal alignment.
    /// Values: "Left", "Center", "Right", "Stretch"
    /// </summary>
    public string HorizontalAlignment { get; set; } = "";

    /// <summary>
    /// Gets or sets the vertical alignment.
    /// Values: "Top", "Center", "Bottom", "Stretch"
    /// </summary>
    public string VerticalAlignment { get; set; } = "";

    /// <summary>
    /// Gets or sets the margin (format: "left,top,right,bottom" or single value for all sides).
    /// </summary>
    public string Margin { get; set; } = "";

    /// <summary>
    /// Gets or sets the padding (format: "left,top,right,bottom" or single value for all sides).
    /// </summary>
    public string Padding { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutAttribute"/> class.
    /// </summary>
    public LayoutAttribute()
    {
    }
}
