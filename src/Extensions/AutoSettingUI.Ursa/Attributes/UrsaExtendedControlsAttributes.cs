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
        BindingProperty = "IPAddress";
        FactoryMethod = "CreateIPv4Box";
    }
}
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CheckBoxAttribute : ControlBindingAttributeBase
{
    public CheckBoxAttribute()
    {
        BindingProperty = "IsChecked";
        FactoryMethod = "CreateCheckBox";
    }
    public CheckBox CreateCheckBox(Type propertyType)
    {
        return new CheckBox();
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TimePickerAttribute : ControlBindingAttributeBase
{
    public TimePickerAttribute()
    {
        
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DatePickerAttribute : ControlBindingAttributeBase
{
    public DatePickerAttribute()
    {
        
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ColorPickerAttribute : ControlBindingAttributeBase
{
    public ColorPickerAttribute()
    {
        
    }
}
