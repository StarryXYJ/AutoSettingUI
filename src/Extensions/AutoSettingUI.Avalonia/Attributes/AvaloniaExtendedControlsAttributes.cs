using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia.Attributes;

/// <summary>
/// Specifies that a boolean property should be edited using a CheckBox control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.CheckBox), "IsChecked")]
public sealed class CheckBoxAttribute()
    : ControlBindingAttribute(typeof(global::Avalonia.Controls.CheckBox), "IsChecked");

/// <summary>
/// Specifies that a Color property should be edited using a ColorPicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.ColorPicker), "Color")]
public sealed class ColorPickerAttribute()
    : ControlBindingAttribute(typeof(global::Avalonia.Controls.ColorPicker), "Color");

/// <summary>
/// Specifies that a TimeSpan or DateTime property should be edited using a TimePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.TimePicker), "SelectedTime")]
public sealed class TimePickerAttribute()
    : ControlBindingAttribute(typeof(global::Avalonia.Controls.TimePicker), "SelectedTime");

/// <summary>
/// Specifies that a DateTime property should be edited using a DatePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.CalendarDatePicker), "SelectedDate")]
public sealed class DatePickerAttribute()
    : ControlBindingAttribute(typeof(global::Avalonia.Controls.CalendarDatePicker), "SelectedDate");
