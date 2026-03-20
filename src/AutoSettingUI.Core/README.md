# AutoSettingUI.Core

Core library for **AutoSettingUI** - A declarative settings UI framework for .NET that automatically generates settings panels from C# classes decorated with attributes.

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Core.svg)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Installation

```xml
<PackageReference Include="AutoSettingUI.Core" />
```

## Features

- ✨ **Attribute-driven** — Decorate properties with attributes to customize rendering
- ⚡ **AOT-compatible** — Works with Native AOT and trimming
- 🔌 **Extensible** — Inject custom providers and accessors

## Quick Start

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

## Available Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `[SettingUI]` | Class | Marks class for UI generation |
| `[MainHeader]` | Class | Sets section header title |
| `[Title]` | Property | Sets property label |
| `[SubHeader]` | Property | Creates a sub-section |
| `[Hide]` | Property | Excludes from UI |
| `[Range]` | Property | Numeric range (renders slider) |
| `[ItemsSource]` | Property | Dropdown items source |
| `[ControlBinding]` | Property | Custom control binding |
| `[ReadOnly]` | Property | Makes property read-only |
| `[Password]` | Property | Masks input |
| `[Placeholder]` | Property | Placeholder text |
| `[Layout]` | Property | Custom layout settings |
| `[Validation]` | Property | Custom validation method |
| `[DisplayOrder]` | Property | Controls display order |

## Related Packages

- **AutoSettingUI.Generator** - Roslyn source generator for AOT support
- **AutoSettingUI.Avalonia** - Avalonia UI panel
- **AutoSettingUI.Ursa** - Ursa-themed Avalonia panel
- **AutoSettingUI.WPF** - WPF panel

## Documentation

- [GitHub Repository](https://github.com/StarryXYJ/AutoSettingUI)
- [Full Documentation](https://github.com/StarryXYJ/AutoSettingUI#readme)

## License

MIT
