# AutoSettingUI.WPF

WPF implementation for **AutoSettingUI** - Declarative settings UI controls for Windows Presentation Foundation applications.

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.WPF.svg)](https://www.nuget.org/packages/AutoSettingUI.WPF/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Installation

```xml
<PackageReference Include="AutoSettingUI.WPF" Version="1.0.0" />
```

## Quick Start

### 1. Define Your Settings Class

```csharp
using AutoSettingUI.Core.Attributes;

[SettingUI(Category = "General")]
[MainHeader("Application Settings")]
public class AppSettings
{
    [Title("App Name")]
    public string Name { get; set; } = "My App";

    [Title("Enable Dark Mode")]
    public bool DarkMode { get; set; }

    [Title("Volume"), Range(0, 100)]
    public int Volume { get; set; } = 50;
}
```

### 2. Add to Your WPF Window

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel
        Title="Settings"
        Targets="{Binding SettingsList}"
        ShowNavigation="True"
        NavigationWidth="200" />
</Window>
```

### 3. AOT Support (Optional)

```csharp
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## Extended Controls

| Attribute | Description |
|-----------|-------------|
| `[CheckBox]` | Boolean as CheckBox |
| `[DatePicker]` | DateTime with date picker |

## Dependency Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string?` | `null` | Panel-level title |
| `Targets` | `IList?` | `null` | Settings objects to render |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null` | Custom provider for AOT |
| `ShowNavigation` | `bool` | `true` | Show sidebar navigation |
| `NavigationWidth` | `double` | `200` | Sidebar width |

## Control Selection Logic

The WPF panel automatically selects controls based on property type and attributes:

| Condition | Control |
|-----------|---------|
| `[Range]` on numeric property | `Slider` |
| `bool` property | `CheckBox` |
| `enum` property | `ComboBox` |
| Property name contains "password" or "pwd" | `PasswordBox` |
| Property named "description", "notes", or "comment" | `TextBox` (multi-line) |
| Other strings/types | `TextBox` (single-line) |
| `[ControlBinding]` attribute | Custom control |

## Custom Control Binding

```csharp
[Title("Volume")]
[ControlBinding(typeof(Slider), BindingProperty = "Value", FactoryMethod = nameof(VolumeFactory))]
public double Volume { get; set; } = 50.0;

public Slider VolumeFactory()
{
    return new Slider
    {
        Minimum = 0,
        Maximum = 100,
        Width = 200,
        TickFrequency = 10,
        IsSnapToTickEnabled = true
    };
}
```

## Related Packages

- **AutoSettingUI.Core** - Core attributes and interfaces
- **AutoSettingUI.Generator** - Roslyn source generator for AOT
- **AutoSettingUI.Avalonia** - Avalonia UI panel
- **AutoSettingUI.Ursa** - Ursa-themed Avalonia panel

## Documentation

- [GitHub Repository](https://github.com/StarryXYJ/AutoSettingUI)
- [Framework Extensions Guide](https://github.com/StarryXYJ/AutoSettingUI/blob/main/manual/extensions.md)

## License

MIT
