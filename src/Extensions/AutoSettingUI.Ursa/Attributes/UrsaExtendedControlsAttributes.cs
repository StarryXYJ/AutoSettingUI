using AutoSettingUI.Core.Attributes;
using Avalonia.Controls;

namespace AutoSettingUI.Ursa.Attributes;

/// <summary>
/// Specifies that a collection of strings should be edited using a TagInput control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Ursa.Controls.TagInput), "Tags")]
public sealed class TagInputAttribute : ControlBindingAttribute
{
    public TagInputAttribute():base(typeof(global::Ursa.Controls.TagInput), "Tags")
    {
    }
}

/// <summary>
/// Specifies that an IPAddress property should be edited using an IPv4Box control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Ursa.Controls.IPv4Box), "IPAddress")]
public sealed class IPv4BoxAttribute() : ControlBindingAttribute(typeof(global::Ursa.Controls.IPv4Box), "IPAddress");

/// <summary>
/// Specifies that a boolean property should be edited using a CheckBox control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(CheckBox), "IsChecked")]
public sealed class CheckBoxAttribute() : ControlBindingAttribute(typeof(CheckBox), "IsChecked");

/// <summary>
/// Specifies that a TimeSpan property should be edited using a TimePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(TimePicker), "SelectedTime")]
public sealed class TimePickerAttribute() : ControlBindingAttribute(typeof(TimePicker), "SelectedTime");

/// <summary>
/// Specifies that a DateTime property should be edited using a CalendarDatePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(CalendarDatePicker), "SelectedDate")]
public sealed class DatePickerAttribute() : ControlBindingAttribute(typeof(CalendarDatePicker), "SelectedDate");

/// <summary>
/// Specifies that a Color property should be edited using a ColorPicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(ColorPicker), "Color")]
public sealed class ColorPickerAttribute() : ControlBindingAttribute(typeof(ColorPicker), "Color");
