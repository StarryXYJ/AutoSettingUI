using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia.Attributes;

/// <summary>
/// Specifies that a numeric property should be rendered using Avalonia's NumericUpDown control.
/// Provides up/down buttons for incrementing/decrementing numeric values.
/// </summary>
/// <example>
/// [NumericUpDown(Minimum = 0, Maximum = 100, Increment = 5)]
/// [Title("Item Count")]
/// public int ItemCount { get; set; } = 10;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.NumericUpDown), "Value", nameof(CreateNumericUpDown))]
public sealed class NumericUpDownAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.NumericUpDown))
{
    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public double Minimum { get; set; } = double.MinValue;

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets the increment/step value for the up/down buttons.
    /// </summary>
    public double Increment { get; set; } = 1.0;

    /// <summary>
    /// Factory method that creates a configured NumericUpDown control.
    /// Called by the UI panel at runtime.
    /// </summary>
    /// <param name="propertyType">The type of the property being edited.</param>
    /// <returns>A configured NumericUpDown control.</returns>
    public global::Avalonia.Controls.Control CreateNumericUpDown(Type propertyType)
    {
        return new global::Avalonia.Controls.NumericUpDown
        {
            Minimum = (decimal)Minimum,
            Maximum = (decimal)Maximum,
            Increment = (decimal)Increment
        };
    }
}
