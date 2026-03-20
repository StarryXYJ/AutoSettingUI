# AutoSettingUI

**AutoSettingUI** is a .NET library that automatically generates settings UI panels from plain C# objects decorated with attributes. Annotate a class, hand it to the control, and a fully functional settings form is rendered — no manual UI wiring required.

## Features

- 🎨 **Multi-framework** — Avalonia, Ursa (Avalonia), and WPF panels included.
- ✨ **Attribute-driven** — Decorate properties with `[Title]`, `[Range]`, `[Hide]`, `[SubHeader]`, `[ItemsSource]`, and `[ControlBinding]` to customise rendering.
- ⚡ **AOT-compatible** — An Incremental Roslyn Source Generator (`AutoSettingUI.Generator`) generates a reflection-free `ISettingDescriptorProvider` + `IPropertyValueAccessor` at compile time, enabling full Native AOT and trimming support.
- 🔌 **Extensible** — Inject your own `ISettingDescriptorProvider` or `IPropertyValueAccessor` to override the defaults.
- 🧭 **Built-in navigation** — Sections are listed in a sidebar tree; clicking navigates directly to the section.
- 🎯 **Extended Controls** — Built-in support for ColorPicker, DatePicker, TimePicker, NumericUpDown, and more.
- 🔄 **MVVM Support** — Works seamlessly with CommunityToolkit.Mvvm `[ObservableProperty]` attribute on partial classes.

## Installation

Install the package for your preferred UI framework. Each package automatically includes the necessary dependencies (`Core`, `Extension.Shared`).

### Avalonia

```bash
dotnet add package AutoSettingUI.Avalonia
```

```xml
<PackageReference Include="AutoSettingUI.Avalonia" Version="1.0.0" />
```

### Ursa (Avalonia with Ursa theme)

```bash
dotnet add package AutoSettingUI.Ursa
```

```xml
<PackageReference Include="AutoSettingUI.Ursa" Version="1.0.0" />
```

### WPF

```bash
dotnet add package AutoSettingUI.WPF
```

```xml
<PackageReference Include="AutoSettingUI.WPF" Version="1.0.0" />
```

### AOT Support (Optional)

For Native AOT or trimming support, also install the source generator:

```bash
dotnet add package AutoSettingUI.Generator
```

```xml
<PackageReference Include="AutoSettingUI.Generator" Version="1.0.0" />
```

**Target frameworks:** `AutoSettingUI.Core`, `AutoSettingUI.Avalonia`, `AutoSettingUI.Ursa`, and `AutoSettingUI.Generator` target `net8.0;net9.0;net10.0`. `AutoSettingUI.WPF` targets `net8.0-windows;net9.0-windows;net10.0-windows`.

**Avalonia version range:** `AutoSettingUI.Avalonia` references `[11.0.0,12.0.0)`. `AutoSettingUI.Ursa` references `[11.1.1,12.0.0)` (Ursa 1.13.0 requires Avalonia >= 11.1.1).

## Quick Start

This section shows a minimal end-to-end setup using the current demos as reference. It covers Avalonia, Ursa, and WPF, plus AOT support.

### 1. Add Packages

Install the package for your UI framework:

```bash
# Avalonia
dotnet add package AutoSettingUI.Avalonia

# Ursa
dotnet add package AutoSettingUI.Ursa

# WPF
dotnet add package AutoSettingUI.WPF
```

For AOT support, also add the generator:

```bash
dotnet add package AutoSettingUI.Generator
```

### 2. Add Style References (Required for Avalonia/Ursa)

> **⚠️ Important:** For Avalonia and Ursa projects, you **must** add the theme style reference to your `App.axaml` file. Without this, the controls will not render correctly.

#### Avalonia

Add to your `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApp.App">
    <Application.Styles>
        <FluentTheme />
        <!-- Required for ColorPicker support (optional) -->
        <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
        <!-- Required for AutoSettingUI.Avalonia -->
        <StyleInclude Source="avares://AutoSettingUI.Avalonia/Themes/Generic.axaml"/>
    </Application.Styles>
</Application>
```

#### Ursa

Add to your `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:u-semi="https://irihi.tech/ursa/themes/semi"
             x:Class="YourApp.App">
    <Application.Styles>
        <u-semi:SemiTheme Locale="zh-CN" />
        <!-- Required for ColorPicker support (optional) -->
        <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
        <!-- Required for AutoSettingUI.Ursa -->
        <StyleInclude Source="avares://AutoSettingUI.Ursa/Themes/Generic.axaml"/>
    </Application.Styles>
</Application>
```

#### WPF

WPF does not require additional style references. The default styles are included automatically.

### 3. Define a Settings Model

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

#### Using with CommunityToolkit.Mvvm

AutoSettingUI supports `[ObservableProperty]` from CommunityToolkit.Mvvm. Use partial classes with private fields:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using AutoSettingUI.Core.Attributes;

[SettingUI]
[MainHeader("Application Settings")]
public partial class ApplicationSettings : ObservableObject
{
    [Title("Application Name")]
    [ObservableProperty]
    private string _appName = "My Application";

    [ObservableProperty]
    private string _version = "1.0.0";

    [Title("Enable Logging")]
    [ObservableProperty]
    private bool _enableLogging;
}
```

> **Note:** The source generator automatically detects fields marked with `[ObservableProperty]` and generates UI for the corresponding properties. Field naming conventions (`_fieldName` or `m_fieldName`) are automatically converted to property names (`FieldName`).

### 4. Bind the Panel

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

### 5. AOT Support (Optional)

For Native AOT or trimming, make sure the generator is referenced by the **app** project. In most cases the generated provider is picked up automatically. If you want to force it (or you see fallback-to-TextBox in AOT), do:

```csharp
using AutoSettingUI.Core.Registry;
using AutoSettingUI.Generated;

var provider = new GeneratedSettingProvider();
AotSettingRegistry.Provider = provider;
AotSettingRegistry.Accessor = provider;
```

### 6. Publish AOT (Example)

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
| `[DisplayOrder]`   | Property  | Controls display order (lower = first)     |

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

### User-Facing Packages (Install These)

| Package                   | Description                                         | When to Use                          |
| ------------------------- | --------------------------------------------------- | ------------------------------------ |
| `AutoSettingUI.Avalonia`  | Avalonia UI panel extension. Includes Core + Shared | Avalonia applications                |
| `AutoSettingUI.Ursa`      | Ursa-themed Avalonia panel extension. Includes all  | Ursa (Avalonia theme) applications   |
| `AutoSettingUI.WPF`       | WPF panel extension. Includes Core + Shared         | WPF applications                     |
| `AutoSettingUI.Generator` | Roslyn Source Generator for AOT support (Optional)  | When using Native AOT or trimming    |

### Internal Dependencies (Auto-Included)

| Package                        | Description                                  |
| ------------------------------ | -------------------------------------------- |
| `AutoSettingUI.Core`           | Attributes, interfaces, and models.          |
| `AutoSettingUI.Extension.Shared` | Shared validation and control helpers.     |

> **Note:** You don't need to install `Core` or `Extension.Shared` manually — they are automatically included when you install any UI framework package.

## Documentation

- [Architecture](manual/architecture.md) — Project structure and data flow
- [Attributes Reference](manual/attributes.md) — All available attributes
- [Framework Extensions](manual/extensions.md) — Framework-specific usage
- [AOT Source Generator](manual/aot-source-generator.md) — AOT support details


## Building and Publishing

For maintainers and contributors:

### Build the Solution

```powershell
# Debug build
.\build-all.ps1

# Release build with NuGet packages
.\build-all.ps1 -Configuration Release -Pack
```

### Publish NuGet Packages

Use the interactive script to select and publish packages:

```powershell
.\publish-nuget.ps1
```

This will show an interactive menu:

```
========================================
  AutoSettingUI NuGet Pack & Publish
========================================

Select packages to pack/publish:

  1. AutoSettingUI.Core - Core library with attributes and descriptors
  2. AutoSettingUI.Generator - Roslyn source generator for AOT support
  3. AutoSettingUI.Avalonia - Avalonia UI controls
  4. AutoSettingUI.Ursa - Ursa UI controls (Avalonia theme)
  5. AutoSettingUI.WPF - WPF UI controls

  A. Pack ALL packages
  Q. Quit
```

Enter numbers (e.g., `1 3 4`) to select specific packages, or `A` for all.

### Publish Demo Applications

```powershell
# Avalonia demo (Normal mode)
.\publish-demo.ps1 -Framework Avalonia -Mode Normal

# Avalonia demo (AOT mode)
.\publish-demo.ps1 -Framework Avalonia -Mode AOT

# Ursa demo
.\publish-demo.ps1 -Framework Ursa -Mode Normal

# WPF demo
.\publish-demo.ps1 -Framework WPF -Mode Normal
```

## License

MIT
