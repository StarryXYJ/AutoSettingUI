# AutoSettingUI.Ursa

Ursa-themed Avalonia UI implementation for **AutoSettingUI** - Declarative settings UI controls with beautiful Ursa styling.

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Ursa.svg)](https://www.nuget.org/packages/AutoSettingUI.Ursa/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Installation

```xml
<PackageReference Include="AutoSettingUI.Ursa" />
```

## Quick Start

### 1. Add Style References (Required)

> **⚠️ Important:** You **must** add the theme style reference to your `App.axaml` file. Without this, the controls will not render correctly.

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

### 2. Define Your Settings Class

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

### 3. Add to Your Avalonia Window

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel
        Title="Settings"
        Targets="{Binding SettingsList}"
        UseCardBorderTheme="True" />
</Window>
```

### 4. AOT Support (Optional)

```csharp
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## Extended Controls

### ColorPicker

> **Important:** Add the ColorPicker styles to your `App.axaml`:

```xml
<Application.Styles>
    <u-semi:SemiTheme Locale="zh-CN" />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
</Application.Styles>
```

Usage:

```csharp
using Avalonia.Media;
using AutoSettingUI.Ursa.Attributes;

[SettingUI]
public class ThemeSettings
{
    [Title("Accent Color")]
    [ColorPicker]
    public Color AccentColor { get; set; } = Colors.DodgerBlue;
}
```

### Ursa-Specific Controls

| Attribute | Description |
|-----------|-------------|
| `[TagInput]` | String collection as tags |
| `[IPv4Box]` | IP address input |
| `[CheckBox]` | Boolean as CheckBox |
| `[DatePicker]` | DateTime with date picker |
| `[TimePicker]` | TimeSpan with time picker |
| `[NumericUpDown]` | Numeric input with up/down |
| `[ColorPicker]` | Color selection |

## Styled Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | `string?` | `null` | Panel-level title |
| `Targets` | `IEnumerable?` | `null` | Settings objects to render |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null` | Custom provider for AOT |
| `PropertyAccessor` | `IPropertyValueAccessor?` | `null` | Custom accessor for AOT |
| `ShowNavigation` | `bool` | `true` | Show sidebar navigation |
| `NavigationWidth` | `double` | `200` | Sidebar width |
| `UseCardBorderTheme` | `bool` | `true` | Use Ursa card borders |

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
        MaxWidth = 200,
        TickFrequency = 10
    };
}
```

## Related Packages

- **AutoSettingUI.Core** - Core attributes and interfaces
- **AutoSettingUI.Generator** - Roslyn source generator for AOT
- **AutoSettingUI.Avalonia** - Plain Avalonia panel
- **AutoSettingUI.WPF** - WPF panel

## Documentation

- [GitHub Repository](https://github.com/StarryXYJ/AutoSettingUI)
- [Framework Extensions Guide](https://github.com/StarryXYJ/AutoSettingUI/blob/main/manual/extensions.md)

## License

MIT
