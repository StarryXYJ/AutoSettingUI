# Framework Extension Guide

This page documents how to use AutoSettingUI in each supported framework.

---

## Avalonia (`AutoSettingUI.Avalonia`)

### NuGet / Project Reference

```xml
<ProjectReference Include="path\to\AutoSettingUI.Avalonia.csproj" />
```

### XAML Usage

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">

    <auto:AvaloniaAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        ShowNavigation="True"
        NavigationWidth="200" />

</Window>
```

### Styled Properties

| Property             | Type                          | Default | Description                               |
| -------------------- | ----------------------------- | ------- | ----------------------------------------- |
| `Title`              | `string?`                     | `null`  | Optional panel-level title.               |
| `Targets`            | `IEnumerable?`                | `null`  | Collection of settings objects to render. |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null`  | Inject the generated provider for AOT.    |
| `PropertyAccessor`   | `IPropertyValueAccessor?`     | `null`  | Inject the generated accessor for AOT.    |
| `ShowNavigation`     | `bool`                        | `true`  | Show/hide the left sidebar navigation.    |
| `NavigationWidth`    | `double`                      | `200`   | Width of the navigation sidebar.          |

---

## Ursa (`AutoSettingUI.Ursa`)

Identical API to the Avalonia panel but styled with Ursa's card-based theme.

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">

    <ursa:UrsaAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        UseCardBorderTheme="True" />

</Window>
```

### Additional Styled Properties

| Property             | Type   | Default | Description                                |
| -------------------- | ------ | ------- | ------------------------------------------ |
| `UseCardBorderTheme` | `bool` | `true`  | Renders sections inside Ursa card borders. |

---

## WPF (`AutoSettingUI.WPF`)

### XAML Usage

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">

    <auto:WpfAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        ShowNavigation="True"
        NavigationWidth="200" />

</Window>
```

### Dependency Properties

| Property             | Type                          | Default | Description                         |
| -------------------- | ----------------------------- | ------- | ----------------------------------- |
| `Title`              | `string?`                     | `null`  | Optional panel-level title.         |
| `Targets`            | `IList?`                      | `null`  | List of settings objects to render. |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null`  | Custom provider.                    |
| `ShowNavigation`     | `bool`                        | `true`  | Show/hide sidebar.                  |
| `NavigationWidth`    | `double`                      | `200`   | Sidebar width.                      |

### WpfControlFactory

The WPF panel delegates control creation to `WpfControlFactory`. It automatically selects:

- `Slider` for `[Range]` numeric properties.
- `CheckBox` for `bool` properties.
- `ComboBox` for `enum` properties.
- `PasswordBox` for properties whose names contain `password` or `pwd`.
- `TextBox` (multi-line) for properties named `description`, `notes`, or `comment`.
- `TextBox` (single-line) for all other strings and types.
- Custom controls via `[ControlBinding]`.

---

## Creating Custom Control Attributes

You can create custom control attributes by inheriting from `ControlBindingAttribute`:

```csharp
// Example: Custom DatePicker attribute for Avalonia
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") 
    {
    }
    
    public string Format { get; set; } = "yyyy-MM-dd";
}
```

### How It Works

1. **ControlType**: Specifies the control type to instantiate.
2. **BindingProperty**: The property on the control to bind to (without "Property" suffix).
3. **FactoryMethod** (optional): Name of a method to create the control instance.

### Factory Method Pattern

For complex controls, define a factory method:

```csharp
public sealed class NumericUpDownAttribute : ControlBindingAttribute
{
    public double Minimum { get; set; } = double.MinValue;
    public double Maximum { get; set; } = double.MaxValue;
    
    public NumericUpDownAttribute() 
        : base(typeof(NumericUpDown), "Value", nameof(CreateNumericUpDown)) 
    {
    }
    
    // Instance method on the attribute - called automatically
    public Control CreateNumericUpDown(Type propertyType)
    {
        return new NumericUpDown
        {
            Minimum = (decimal)Minimum,
            Maximum = (decimal)Maximum
        };
    }
}
```

### Automatic Control Creation

The `CreateExtendedControl` method in Avalonia/Ursa panels:
1. Checks for `ControlBindingAttribute` on the property
2. If `FactoryMethod` is specified, tries to invoke it (static on control type, then instance on attribute)
3. Falls back to `Activator.CreateInstance` if no factory method
4. Automatically binds to the specified `BindingProperty`
