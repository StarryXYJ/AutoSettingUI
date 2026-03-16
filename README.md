# AutoSettingUI

**AutoSettingUI** is a .NET library that automatically generates settings UI panels from plain C# objects decorated with attributes. Annotate a class, hand it to the control, and a fully functional settings form is rendered — no manual UI wiring required.

## Features

- 🎨 **Multi-framework** — Avalonia, Ursa (Avalonia), and WPF panels included.
- ✨ **Attribute-driven** — Decorate properties with `[Title]`, `[Range]`, `[Hide]`, `[SubHeader]`, `[ItemsSource]`, and `[ControlBinding]` to customise rendering.
- ⚡ **AOT-compatible** — An Incremental Roslyn Source Generator (`AutoSettingUI.Generator`) generates a reflection-free `ISettingDescriptorProvider` + `IPropertyValueAccessor` at compile time, enabling full Native AOT and trimming support.
- 🔌 **Extensible** — Inject your own `ISettingDescriptorProvider` or `IPropertyValueAccessor` to override the defaults.
- 🧭 **Built-in navigation** — Sections are listed in a sidebar tree; clicking navigates directly to the section.
- 🎯 **Extended Controls** — Built-in support for ColorPicker, DatePicker, TimePicker, NumericUpDown, and more.

## Installation

Install the package for your preferred UI framework:

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" Version="1.0.0" />

<!-- Ursa (Avalonia with Ursa theme) -->
<PackageReference Include="AutoSettingUI.Ursa" Version="1.0.0" />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF" Version="1.0.0" />
```

> **Note:** `AutoSettingUI.Core` and `AutoSettingUI.Generator` are automatically included as dependencies.

## Quick Start

### 1. Define Your Settings Class

```csharp
using AutoSettingUI.Core.Attributes;

[SettingUI(Category = "General", Order = 0)]
[MainHeader("Application Settings")]
public class AppSettings
{
    [Title("App Name")]
    public string Name { get; set; } = "My App";

    [Title("Enable Dark Mode")]
    public bool DarkMode { get; set; }

    [Title("Volume"), Range(0, 100)]
    public int Volume { get; set; } = 50;

    [Title("Language")]
    [ItemsSource(typeof(AppSettings), nameof(AvailableLanguages))]
    public string Language { get; set; } = "en-US";

    public static string[] AvailableLanguages => ["en-US", "zh-CN", "ja-JP"];
}
```

### 2. Add the Panel to Your UI

#### Avalonia

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">
    <auto:AvaloniaAutoSettingPanel
        Title="Settings"
        Targets="{Binding SettingsList}"
        ShowNavigation="True" />
</Window>
```

#### Ursa

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel
        Title="Settings"
        Targets="{Binding SettingsList}"
        UseCardBorderTheme="True" />
</Window>
```

#### WPF

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel
        Title="Settings"
        Targets="{Binding SettingsList}"
        ShowNavigation="True" />
</Window>
```

### 3. AOT Support (Optional)

For Native AOT or trimming support, use the generated provider:

```csharp
// The source generator creates this class automatically
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## Extended Controls

AutoSettingUI provides extended control attributes for specialized inputs:

### ColorPicker (Avalonia/Ursa)

> **Important:** ColorPicker requires additional style reference in `App.axaml`:

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
</Application.Styles>
```

Usage:

```csharp
using Avalonia.Media;
using AutoSettingUI.Avalonia.Attributes; // or AutoSettingUI.Ursa.Attributes

[SettingUI]
public class ThemeSettings
{
    [Title("Accent Color")]
    [ColorPicker]
    public Color AccentColor { get; set; } = Colors.DodgerBlue;
}
```

### Other Extended Controls

| Attribute          | Framework | Description                          |
| ------------------ | --------- | ------------------------------------ |
| `[CheckBox]`       | All       | Boolean property as CheckBox         |
| `[DatePicker]`     | All       | DateTime with date picker            |
| `[TimePicker]`     | All       | TimeSpan with time picker            |
| `[NumericUpDown]`  | Avalonia  | Numeric input with up/down buttons   |
| `[ColorPicker]`    | Avalonia  | Color selection                      |
| `[TagInput]`       | Ursa      | String collection as tags            |
| `[IPv4Box]`        | Ursa      | IP address input                     |

## Available Attributes

| Attribute          | Target    | Description                                |
| ------------------ | --------- | ------------------------------------------ |
| `[SettingUI]`      | Class     | Marks class for UI generation              |
| `[MainHeader]`     | Class     | Sets section header title                  |
| `[Title]`          | Property  | Sets property label                        |
| `[SubHeader]`      | Property  | Creates a sub-section                      |
| `[Hide]`           | Property  | Excludes from UI                           |
| `[Range]`          | Property  | Numeric range (renders slider)             |
| `[ItemsSource]`    | Property  | Dropdown items source                      |
| `[ControlBinding]` | Property  | Custom control binding                     |
| `[ReadOnly]`       | Property  | Makes property read-only                   |
| `[Password]`       | Property  | Masks input (password box)                 |
| `[Placeholder]`    | Property  | Placeholder text for input                 |
| `[Layout]`         | Property  | Custom layout (width, height)              |
| `[Validation]`     | Property  | Custom validation method                   |

## Custom Control Binding

Create custom control attributes by inheriting from `ControlBindingAttribute`:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") 
    {
    }
}
```

For complex controls, use factory methods:

```csharp
public sealed class NumericUpDownAttribute : ControlBindingAttribute
{
    public double Minimum { get; set; } = 0;
    public double Maximum { get; set; } = 100;
    
    public NumericUpDownAttribute() 
        : base(typeof(NumericUpDown), "Value", nameof(CreateControl)) 
    {
    }
    
    public Control CreateControl(Type propertyType)
    {
        return new NumericUpDown
        {
            Minimum = (decimal)Minimum,
            Maximum = (decimal)Maximum
        };
    }
}
```

## Packages

| Package                      | Description                                     |
| ---------------------------- | ----------------------------------------------- |
| `AutoSettingUI.Core`         | Attributes, interfaces, and models.             |
| `AutoSettingUI.Generator`    | Roslyn Incremental Source Generator (Analyzer). |
| `AutoSettingUI.Extension.Shared` | Shared validation and control helpers.      |
| `AutoSettingUI.Avalonia`     | Avalonia UI panel extension.                    |
| `AutoSettingUI.Ursa`         | Ursa-themed Avalonia panel extension.           |
| `AutoSettingUI.WPF`          | WPF panel extension.                            |

## Documentation

- [Architecture](manual/architecture.md) — Project structure and data flow
- [Attributes Reference](manual/attributes.md) — All available attributes
- [Framework Extensions](manual/extensions.md) — Framework-specific usage
- [AOT Source Generator](manual/aot-source-generator.md) — AOT support details


## License

MIT
