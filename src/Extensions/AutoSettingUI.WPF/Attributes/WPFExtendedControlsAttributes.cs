using System.Windows.Controls;
using AutoSettingUI.Core.Attributes;


namespace AutoSettingUI.WPF.Attributes;



/// <summary>
/// Specifies that a boolean property should be edited using a CheckBox control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CheckBoxAttribute() : ControlBindingAttribute(typeof(CheckBox), "IsChecked");



/// <summary>
/// Specifies that a DateTime property should be edited using a CalendarDatePicker control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DatePickerAttribute() : ControlBindingAttribute(typeof(DatePicker),"SelectedDate");

