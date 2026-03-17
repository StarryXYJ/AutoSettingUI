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

This section shows a minimal end-to-end setup using the current demos as reference. It covers Avalonia, Ursa, and WPF, plus AOT support.

### 1. Add Packages / References

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" Version="1.0.0" />

<!-- Ursa (Avalonia with Ursa theme) -->
<PackageReference Include="AutoSettingUI.Ursa" Version="1.0.0" />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF" Version="1.0.0" />
```

If you want AOT support, also reference the generator in your **app** project:

```xml
<ProjectReference Include="..\..\src\AutoSettingUI.Generator\AutoSettingUI.Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### 2. Define a Settings Model

```csharp
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Avalonia.Attributes; // or AutoSettingUI.Ursa.Attributes

[SettingUI]
public sealed class AppSettings
{
    [Title("Volume")]
    [ControlBinding(typeof(Avalonia.Controls.Slider), "Value", nameof(CreateVolume))]
    public double Volume { get; set; } = 50;

    public Avalonia.Controls.Slider CreateVolume()
        => new Avalonia.Controls.Slider { Minimum = 0, Maximum = 100, Value = 50 };

    [Title("Enable Feature")]
    [CheckBox]
    public bool EnableFeature { get; set; } = true;

    [Title("Font Size")]
    [NumericUpDown(Minimum = 8, Maximum = 32, Increment = 1)]
    public int FontSize { get; set; } = 14;

    [Title("Tags")]
    public System.Collections.ObjectModel.ObservableCollection<string> Tags { get; set; }
        = new() { "Important", "Work" };
}
```

### 3. Bind the Panel

Avalonia:

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">
    <auto:AvaloniaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

Ursa:

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

WPF:

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

ViewModel:

```csharp
public class MainViewModel
{
    public IEnumerable<object> Targets { get; } = new object[]
    {
        new AppSettings()
    };
}
```

### 4. AOT Support (Optional)

For Native AOT or trimming, make sure the generator is referenced by the **app** project. In most cases the generated provider is picked up automatically. If you want to force it (or you see fallback-to-TextBox in AOT), do:

```csharp
using AutoSettingUI.Core.Registry;
using AutoSettingUI.Generated;

var provider = new GeneratedSettingProvider();
AotSettingRegistry.Provider = provider;
AotSettingRegistry.Accessor = provider;
```

### 5. Publish AOT (Example)

```bash
# Ursa demo
pwsh ./publish-demo.ps1 -Framework Avalonia -Mode AOT
```

### Notes

- Use `EmitCompilerGeneratedFiles=true` if you want to inspect generated code.
- `CollectionEditor(AllowEditItems = false)` can make collection rows read-only.

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
