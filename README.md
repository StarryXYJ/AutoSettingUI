# AutoSettingUI

**AutoSettingUI** is a .NET library that automatically generates settings UI panels from plain C# objects decorated with attributes. Annotate a class, hand it to the control, and a fully functional settings form is rendered — no manual UI wiring required.

## Features

- 🎨 **Multi-framework** — Avalonia, Ursa (Avalonia), and WPF panels included.
- ✨ **Attribute-driven** — Decorate properties with `[Title]`, `[Range]`, `[Hide]`, `[SubHeader]`, `[ItemsSource]`, and `[ControlBinding]` to customise rendering.
- ⚡ **AOT-compatible** — An Incremental Roslyn Source Generator (`AutoSettingUI.Generator`) generates a reflection-free `ISettingDescriptorProvider` + `IPropertyValueAccessor` at compile time, enabling full Native AOT and trimming support.
- 🔌 **Extensible** — Inject your own `ISettingDescriptorProvider` or `IPropertyValueAccessor` to override the defaults.
- 🧭 **Built-in navigation** — Sections are listed in a sidebar tree; clicking navigates directly to the section.

## Quick Start

```csharp
[SettingUI(Category = "General", Order = 0)]
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

```xml
<!-- Avalonia -->
<avalonia:AvaloniaAutoSettingPanel
    Targets="{Binding Settings}"
    Title="Application Settings" />
```

The source generator will automatically create `GeneratedSettingProvider` in your project. Pass it in for full AOT:

```csharp
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor   = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## Packages

| Package                   | Description                                     |
| ------------------------- | ----------------------------------------------- |
| `AutoSettingUI.Core`      | Attributes, interfaces, and models.             |
| `AutoSettingUI.Generator` | Roslyn Incremental Source Generator (Analyzer). |
| `AutoSettingUI.Avalonia`  | Avalonia UI panel extension.                    |
| `AutoSettingUI.Ursa`      | Ursa-themed Avalonia panel extension.           |
| `AutoSettingUI.WPF`       | WPF panel extension.                            |

## License

MIT
