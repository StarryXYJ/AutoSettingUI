using System.Windows.Controls;
using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.WPF.Attributes;

/// <summary>
/// Specifies that a boolean property should be rendered as a CheckBox control.
/// </summary>
/// <example>
/// [CheckBox]
/// [Title("Enable Feature")]
/// public bool IsEnabled { get; set; } = true;
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class CheckBoxAttribute() : ControlBindingAttribute(typeof(CheckBox), "IsChecked");

/// <summary>
/// Specifies that a DateTime property should be rendered using a DatePicker control.
/// Allows users to select a date from a calendar popup.
/// </summary>
/// <example>
/// [DatePicker]
/// [Title("Birth Date")]
/// public DateTime BirthDate { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DatePickerAttribute() : ControlBindingAttribute(typeof(DatePicker), "SelectedDate");
