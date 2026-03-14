namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a numeric range for a property.
/// Used by numeric input controls like sliders.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class RangeAttribute : Attribute
{
    /// <summary>
    /// Gets the minimum value of the range.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the maximum value of the range.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets or sets the increment/step value.
    /// </summary>
    public double Step { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    public RangeAttribute(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }
}
