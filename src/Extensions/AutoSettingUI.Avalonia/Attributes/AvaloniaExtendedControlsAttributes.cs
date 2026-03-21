using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia.Attributes;

/// <summary>
/// Specifies that a boolean property should be rendered as a CheckBox control.
/// </summary>
/// <example>
/// [CheckBox]
/// [Title("Enable Feature")]
/// public bool IsEnabled { get; set; } = true;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.CheckBox), "IsChecked")]
public sealed class CheckBoxAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.CheckBox), "IsChecked");

/// <summary>
/// Specifies that a Color property should be rendered using a ColorPicker control.
/// Allows users to select a color from a color palette.
/// </summary>
/// <example>
/// [ColorPicker]
/// [Title("Accent Color")]
/// public Color AccentColor { get; set; } = Colors.DodgerBlue;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.ColorPicker), "Color")]
public sealed class ColorPickerAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.ColorPicker), "Color");

/// <summary>
/// Specifies that a TimeSpan property should be rendered using a TimePicker control.
/// Allows users to select a time of day.
/// </summary>
/// <example>
/// [TimePicker]
/// [Title("Preferred Time")]
/// public TimeSpan PreferredTime { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.TimePicker), "SelectedTime")]
public sealed class TimePickerAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.TimePicker), "SelectedTime");

/// <summary>
/// Specifies that a DateTime property should be rendered using a CalendarDatePicker control.
/// Allows users to select a date from a calendar popup.
/// </summary>
/// <example>
/// [DatePicker]
/// [Title("Birth Date")]
/// public DateTime BirthDate { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Avalonia.Controls.CalendarDatePicker), "SelectedDate")]
public sealed class DatePickerAttribute() : ControlBindingAttribute(typeof(global::Avalonia.Controls.CalendarDatePicker), "SelectedDate");
