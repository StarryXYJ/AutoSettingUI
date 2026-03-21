using AutoSettingUI.Core.Attributes;
using Avalonia.Controls;

namespace AutoSettingUI.Ursa.Attributes;

/// <summary>
/// Specifies that a string collection property should be rendered using Ursa's TagInput control.
/// Ideal for editing lists of tags, keywords, or labels.
/// </summary>
/// <example>
/// [TagInput]
/// [Title("Tags")]
/// public ObservableCollection&lt;string&gt; Tags { get; set; } = new();
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Ursa.Controls.TagInput), "Tags")]
public sealed class TagInputAttribute : ControlBindingAttribute
{
    public TagInputAttribute() : base(typeof(global::Ursa.Controls.TagInput), "Tags")
    {
    }
}

/// <summary>
/// Specifies that an IPAddress property should be rendered using Ursa's IPv4Box control.
/// Provides a specialized input for IPv4 addresses with validation.
/// </summary>
/// <example>
/// [IPv4Box]
/// [Title("Server IP")]
/// public IPAddress ServerIP { get; set; } = IPAddress.Parse("192.168.1.1");
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(global::Ursa.Controls.IPv4Box), "IPAddress")]
public sealed class IPv4BoxAttribute() : ControlBindingAttribute(typeof(global::Ursa.Controls.IPv4Box), "IPAddress");

/// <summary>
/// Specifies that a boolean property should be rendered as a CheckBox control.
/// </summary>
/// <example>
/// [CheckBox]
/// [Title("Enable Feature")]
/// public bool IsEnabled { get; set; } = true;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
[ControlBindingDefaults(typeof(CheckBox), "IsChecked")]
public sealed class CheckBoxAttribute() : ControlBindingAttribute(typeof(CheckBox), "IsChecked");

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
[ControlBindingDefaults(typeof(TimePicker), "SelectedTime")]
public sealed class TimePickerAttribute() : ControlBindingAttribute(typeof(TimePicker), "SelectedTime");

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
[ControlBindingDefaults(typeof(CalendarDatePicker), "SelectedDate")]
public sealed class DatePickerAttribute() : ControlBindingAttribute(typeof(CalendarDatePicker), "SelectedDate");

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
[ControlBindingDefaults(typeof(ColorPicker), "Color")]
public sealed class ColorPickerAttribute() : ControlBindingAttribute(typeof(ColorPicker), "Color");
