# AutoSettingUI

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Core?label=Core)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Avalonia?label=Avalonia)](https://www.nuget.org/packages/AutoSettingUI.Avalonia/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Ursa?label=Ursa)](https://www.nuget.org/packages/AutoSettingUI.Ursa/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.WPF?label=WPF)](https://www.nuget.org/packages/AutoSettingUI.WPF/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Generator?label=Generator)](https://www.nuget.org/packages/AutoSettingUI.Generator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AutoSettingUI.Core?label=Downloads)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![License](https://img.shields.io/github/license/your-org/AutoSettingUI)](LICENSE)

**AutoSettingUI** is a .NET library that automatically generates settings UI panels from plain C# objects decorated with attributes. Annotate a class, hand it to the control, and a fully functional settings form is rendered — no manual UI wiring required.

## Features

- 🎨 **Multi-framework** — Avalonia, Ursa (Avalonia), and WPF panels included
- ✨ **Attribute-driven** — Decorate properties with attributes to customize rendering
- ⚡ **AOT-compatible** — Incremental Roslyn Source Generator enables full Native AOT and trimming support
- 🔄 **MVVM Support** — Works with CommunityToolkit.Mvvm `[ObservableProperty]`
- 🔌 **Extensible** — Inject custom providers and accessors
- 🧭 **Built-in navigation** — Sidebar tree navigation to sections
- 🎯 **Extended Controls** — ColorPicker, DatePicker, TimePicker, NumericUpDown, and more

## Installation

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" />

<!-- Ursa (Avalonia with Ursa theme) -->
<PackageReference Include="AutoSettingUI.Ursa" />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF" />

<!-- AOT Support (Optional) -->
<PackageReference Include="AutoSettingUI.Generator" />
```

**Target Frameworks:** `net8.0; net9.0; net10.0` (WPF: `net8.0-windows; net9.0-windows; net10.0-windows`)

**Avalonia:** `AutoSettingUI.Avalonia` requires Avalonia >= 11.0.0. `AutoSettingUI.Ursa` requires Avalonia >= 11.1.1 (Ursa dependency).

## Quick Start

### 1. Add Style References (Avalonia/Ursa only)

> **⚠️ Important:** For Avalonia and Ursa projects, add the theme style reference to `App.axaml`.

**Avalonia:**
```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
    <StyleInclude Source="avares://AutoSettingUI.Avalonia/Themes/Generic.axaml"/>
</Application.Styles>
```

**Ursa:**
```xml
<Application.Styles>
    <u-semi:SemiTheme Locale="zh-CN" />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
    <StyleInclude Source="avares://AutoSettingUI.Ursa/Themes/Generic.axaml"/>
</Application.Styles>
```

### 2. Define a Settings Model

```csharp
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Avalonia.Attributes; // or AutoSettingUI.Ursa.Attributes

[SettingUI]
[MainHeader("Application Settings")]
public class AppSettings
{
    [Title("Application Name")]
    public string AppName { get; set; } = "My Application";

    [Title("Volume")]
    [Range(0, 100)]
    public int Volume { get; set; } = 50;

    [Title("Enable Logging")]
    [CheckBox]
    public bool EnableLogging { get; set; }

    [Title("Font Size")]
    [NumericUpDown(Minimum = 8, Maximum = 32)]
    public int FontSize { get; set; } = 14;
}
```

#### Using with CommunityToolkit.Mvvm

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

[SettingUI]
[MainHeader("Application Settings")]
public partial class ApplicationSettings : ObservableObject
{
    [Title("Application Name")]
    [ObservableProperty]
    private string _appName = "My Application";

    [Title("Enable Logging")]
    [ObservableProperty]
    private bool _enableLogging;
}
```

### 3. Bind the Panel

**Avalonia:**
```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">
    <auto:AvaloniaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**Ursa:**
```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**WPF:**
```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**ViewModel:**
```csharp
public class MainViewModel
{
    public IEnumerable<object> Targets { get; } = new object[] { new AppSettings() };
}
```

### 4. AOT Support (Optional)

For Native AOT or trimming, the generator is picked up automatically. To force registration:

```csharp
using AutoSettingUI.Core.Registry;
using AutoSettingUI.Generated;

AotSettingRegistry.Provider = new GeneratedSettingProvider();
AotSettingRegistry.Accessor = AotSettingRegistry.Provider;
```

## Available Attributes

| Attribute          | Target   | Description                              |
| ------------------ | -------- | ---------------------------------------- |
| `[SettingUI]`      | Class    | Marks class for UI generation            |
| `[MainHeader]`     | Class    | Sets section header title                |
| `[Title]`          | Property | Sets property label                      |
| `[SubHeader]`      | Property | Creates a sub-section                    |
| `[Hide]`           | Property | Excludes from UI                         |
| `[Range]`          | Property | Numeric range (renders slider)           |
| `[ItemsSource]`    | Property | Dropdown items source                    |
| `[ControlBinding]` | Property | Custom control binding                   |
| `[ReadOnly]`       | Property | Makes property read-only                 |
| `[Password]`       | Property | Masks input (password box)               |
| `[Placeholder]`    | Property | Placeholder text for input               |
| `[Layout]`         | Property | Custom layout (width, height)            |
| `[Validation]`     | Property | Custom validation method                 |
| `[DisplayOrder]`   | Property | Controls display order (lower = first)   |

## Extended Controls

| Attribute         | Framework | Description                        |
| ----------------- | --------- | ---------------------------------- |
| `[CheckBox]`      | All       | Boolean property as CheckBox       |
| `[DatePicker]`    | All       | DateTime with date picker          |
| `[TimePicker]`    | All       | TimeSpan with time picker          |
| `[NumericUpDown]` | Avalonia  | Numeric input with up/down buttons |
| `[ColorPicker]`   | Avalonia  | Color selection                    |
| `[TagInput]`      | Ursa      | String collection as tags          |
| `[IPv4Box]`       | Ursa      | IP address input                   |

> **Note:** ColorPicker requires `<StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>` in `App.axaml`.

## Custom Control Binding

Create custom control attributes by inheriting from `ControlBindingAttribute`:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") { }
}
```

For complex controls, use factory methods:

```csharp
public sealed class NumericUpDownAttribute : ControlBindingAttribute
{
    public double Minimum { get; set; }
    public double Maximum { get; set; } = 100;

    public NumericUpDownAttribute() 
        : base(typeof(NumericUpDown), "Value", nameof(CreateControl)) { }

    public Control CreateControl(Type propertyType) => new NumericUpDown
    {
        Minimum = (decimal)Minimum,
        Maximum = (decimal)Maximum
    };
}
```

## Packages

| Package                   | Description                      | Dependencies              |
| ------------------------- | -------------------------------- | ------------------------- |
| `AutoSettingUI.Avalonia`  | Avalonia UI panel                | Core, Extension.Shared    |
| `AutoSettingUI.Ursa`      | Ursa-themed Avalonia panel       | Core, Extension.Shared    |
| `AutoSettingUI.WPF`       | WPF panel                        | Core, Extension.Shared    |
| `AutoSettingUI.Generator` | Roslyn Source Generator for AOT  | Standalone (Analyzer)     |
| `AutoSettingUI.Core`      | Attributes, interfaces, models   | Standalone                |

> **Note:** `Core` and `Extension.Shared` are automatically included when installing UI framework packages.

## Documentation

- [Architecture](manual/architecture.md) — Project structure and data flow
- [Attributes Reference](manual/attributes.md) — All available attributes
- [Framework Extensions](manual/extensions.md) — Framework-specific usage
- [AOT Source Generator](manual/aot-source-generator.md) — AOT support details

## Building

```powershell
# Debug build
.\build-all.ps1

# Release build with NuGet packages
.\build-all.ps1 -Configuration Release -Pack

# Publish demo applications
.\publish-demo.ps1 -Framework Avalonia -Mode AOT
.\publish-demo.ps1 -Framework Ursa -Mode Normal
.\publish-demo.ps1 -Framework WPF -Mode Normal
```

## License

MIT
