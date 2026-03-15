using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Ursa.Attributes;

/// <summary>
/// Specifies that a numeric property should be edited using a NumericUpDown control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NumericUpDownAttribute : ControlBindingAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericUpDownAttribute"/> class.
    /// This is used by the source generator to identify properties that should use NumericUpDown.
    /// </summary>
    public NumericUpDownAttribute()
    {
        // For Ursa, the specific control type depends on the property type (NumericIntUpDown, etc.)
        // We set the factory method here so the source generator can pick it up.
        FactoryMethod = "CreateNumericUpDown";
        BindingProperty = "Value";

    }

    public double Minimum { get; set; } = double.MinValue;
    public double Maximum { get; set; } = double.MaxValue;
    public double Increment { get; set; } = 1.0;

    /// <summary>
    /// Factory method used to create the appropriate NumericUpDown control based on property type.
    /// This is invoked by the UI panel at runtime.
    /// </summary>
    public Avalonia.Controls.Control CreateNumericUpDown(Type propertyType)
    {
        var controlType = GetUrsaNumericUpDownType(propertyType);
        
        var control = (Avalonia.Controls.Control)Activator.CreateInstance(controlType)!;

        // Apply customization
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
        // ... (Add other types if needed, but these are the main ones)

        return control;
    }

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

        return typeof(global::Ursa.Controls.NumericIntUpDown); // Default fallback
    }
}
