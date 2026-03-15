using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia.Attributes;

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

/// <summary>
/// Specifies that a TimeSpan or DateTime property should be edited using a TimePicker control.
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
/// Specifies that a DateTime property should be edited using a DatePicker control.
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
