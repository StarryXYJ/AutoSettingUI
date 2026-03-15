using AutoSettingUI.Core.Attributes;
using Avalonia.Controls;

namespace AutoSettingUI.Ursa.Attributes;

/// <summary>
/// Specifies that a collection of strings should be edited using a TagInput control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TagInputAttribute : ControlBindingAttributeBase
{
    public TagInputAttribute()
    {
        ControlType = typeof(global::Ursa.Controls.TagInput);
        BindingProperty = "Tags";
        FactoryMethod = "CreateTagInput";
    }
}

/// <summary>
/// Specifies that an IPAddress property should be edited using an IPv4Box control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class IPv4BoxAttribute : ControlBindingAttributeBase
{
    public IPv4BoxAttribute()
    {
        ControlType = typeof(global::Ursa.Controls.IPv4Box);
        BindingProperty = "IPAddress";
        FactoryMethod = "CreateIPv4Box";
    }
}

/// <summary>
/// Specifies that a boolean property should be edited using a CheckBox control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CheckBoxAttribute : ControlBindingAttributeBase
{
    public CheckBoxAttribute()
    {
        ControlType = typeof(global::Avalonia.Controls.CheckBox);
        BindingProperty = "IsChecked";
        FactoryMethod = "CreateCheckBox";
    }
}

/// <summary>
/// Specifies that a TimeSpan property should be edited using a TimePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TimePickerAttribute : ControlBindingAttributeBase
{
    public TimePickerAttribute()
    {
        ControlType = typeof(global::Avalonia.Controls.TimePicker);
        BindingProperty = "SelectedTime";
        FactoryMethod = "CreateTimePicker";
    }
}

/// <summary>
/// Specifies that a DateTime property should be edited using a CalendarDatePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DatePickerAttribute : ControlBindingAttributeBase
{
    public DatePickerAttribute()
    {
        ControlType = typeof(global::Avalonia.Controls.CalendarDatePicker);
        BindingProperty = "SelectedDate";
        FactoryMethod = "CreateDatePicker";
    }
}

/// <summary>
/// Specifies that a Color property should be edited using a ColorPicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColorPickerAttribute : ControlBindingAttributeBase
{
    public ColorPickerAttribute()
    {
        ControlType = typeof(global::Avalonia.Controls.ColorPicker);
        BindingProperty = "Color";
        FactoryMethod = "CreateColorPicker";
    }
}
