using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia.Attributes;

/// <summary>
/// Specifies that a numeric property should be edited using a NumericUpDown control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.NumericUpDown), "Value", nameof(CreateNumericUpDown))]
public sealed class NumericUpDownAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.NumericUpDown))
{
    public double Minimum { get; set; } = double.MinValue;
    public double Maximum { get; set; } = double.MaxValue;
    public double Increment { get; set; } = 1.0;

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
