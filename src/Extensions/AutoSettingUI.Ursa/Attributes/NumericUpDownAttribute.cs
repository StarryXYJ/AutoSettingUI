using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Ursa.Attributes;

/// <summary>
/// Specifies that a numeric property should be rendered using Ursa's NumericUpDown control.
/// Automatically selects the appropriate control type based on the property type
/// (int, double, float, byte, etc.).
/// </summary>
/// <example>
/// [NumericUpDown(Minimum = 0, Maximum = 100, Increment = 5)]
/// [Title("Item Count")]
/// public int ItemCount { get; set; } = 10;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Ursa.Controls.NumericIntUpDown), "Value", nameof(CreateNumericUpDown))]
public sealed class NumericUpDownAttribute : ControlBindingAttribute
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

    public NumericUpDownAttribute() : base(typeof(global::Ursa.Controls.NumericIntUpDown), "Value", nameof(CreateNumericUpDown))
    {
    }

    /// <summary>
    /// Factory method that creates the appropriate NumericUpDown control based on property type.
    /// Called by the UI panel at runtime.
    /// </summary>
    /// <param name="propertyType">The type of the property being edited.</param>
    /// <returns>A configured NumericUpDown control.</returns>
    public Avalonia.Controls.Control CreateNumericUpDown(Type propertyType)
    {
        var controlType = GetUrsaNumericUpDownType(propertyType);
        var control = (Avalonia.Controls.Control)Activator.CreateInstance(controlType)!;

        if (control is global::Ursa.Controls.NumericIntUpDown intControl)
        {
            intControl.Minimum = (int)Minimum;
            intControl.Maximum = (int)Maximum;
            intControl.Step = (int)Increment;
        }
        else if (control is global::Ursa.Controls.NumericDoubleUpDown doubleControl)
        {
            doubleControl.Minimum = Minimum;
            doubleControl.Maximum = Maximum;
            doubleControl.Step = Increment;
        }

        return control;
    }

    /// <summary>
    /// Maps a .NET numeric type to the corresponding Ursa NumericUpDown control type.
    /// </summary>
    private static Type GetUrsaNumericUpDownType(Type type)
    {
        if (type == typeof(int) || type == typeof(int?)) return typeof(global::Ursa.Controls.NumericIntUpDown);
        if (type == typeof(uint) || type == typeof(uint?)) return typeof(global::Ursa.Controls.NumericUIntUpDown);
        if (type == typeof(double) || type == typeof(double?)) return typeof(global::Ursa.Controls.NumericDoubleUpDown);
        if (type == typeof(float) || type == typeof(float?)) return typeof(global::Ursa.Controls.NumericFloatUpDown);
        if (type == typeof(byte) || type == typeof(byte?)) return typeof(global::Ursa.Controls.NumericByteUpDown);
        if (type == typeof(sbyte) || type == typeof(sbyte?)) return typeof(global::Ursa.Controls.NumericSByteUpDown);
        if (type == typeof(short) || type == typeof(short?)) return typeof(global::Ursa.Controls.NumericShortUpDown);
        if (type == typeof(ushort) || type == typeof(ushort?)) return typeof(global::Ursa.Controls.NumericUShortUpDown);
        if (type == typeof(long) || type == typeof(long?)) return typeof(global::Ursa.Controls.NumericLongUpDown);
        if (type == typeof(ulong) || type == typeof(ulong?)) return typeof(global::Ursa.Controls.NumericULongUpDown);

        return typeof(global::Ursa.Controls.NumericIntUpDown);
    }
}
