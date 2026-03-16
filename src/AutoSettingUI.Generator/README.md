# AutoSettingUI.Generator

Roslyn Incremental Source Generator for **AutoSettingUI** - Generates AOT-compatible setting descriptors at compile time.

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Generator.svg)](https://www.nuget.org/packages/AutoSettingUI.Generator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Installation

```xml
<PackageReference Include="AutoSettingUI.Generator" Version="1.0.0" />
```

## What It Does

This source generator automatically generates:

- `GeneratedSettingProvider` - Implements `ISettingDescriptorProvider`
- `GeneratedPropertyAccessor` - Implements `IPropertyValueAccessor`

These generated classes enable **full Native AOT and trimming support** by eliminating runtime reflection.

## How It Works

1. At compile time, the generator scans for classes decorated with `[SettingUI]`
2. It generates strongly-typed descriptor and accessor classes
3. No runtime reflection is needed to access property metadata or values

## Generated Code Example

For a settings class like:

```csharp
[SettingUI]
public class AppSettings
{
    [Title("Volume")]
    public int Volume { get; set; } = 50;
}
```

The generator produces (simplified):

```csharp
public partial class GeneratedSettingProvider : ISettingDescriptorProvider, IPropertyValueAccessor
{
    public SettingClassDescriptor? GetDescriptor(Type type)
    {
        if (type == typeof(AppSettings))
            return GetAppSettingsDescriptor();
        // ... other types
    }

    private SettingClassDescriptor GetAppSettingsDescriptor()
    {
        return new SettingClassDescriptor
        {
            TargetType = typeof(AppSettings),
            Properties = new[]
            {
                new PropertyDescriptor
                {
                    PropertyName = "Volume",
                    PropertyType = typeof(int),
                    // ... metadata from attributes
                }
            }
        };
    }

    public object? GetValue(object target, string propertyName)
    {
        if (target is AppSettings settings)
        {
            return propertyName switch
            {
                "Volume" => settings.Volume,
                // ... other properties
            };
        }
        return null;
    }
}
```

## Usage

```csharp
// The generated class is in the AutoSettingUI.Generated namespace
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## Requirements

- .NET Standard 2.0+ compatible projects
- C# 9.0+ (for source generators support)

## Related Packages

- **AutoSettingUI.Core** - Core attributes and interfaces
- **AutoSettingUI.Avalonia** - Avalonia UI panel
- **AutoSettingUI.Ursa** - Ursa-themed Avalonia panel
- **AutoSettingUI.WPF** - WPF panel

## Documentation

- [GitHub Repository](https://github.com/StarryXYJ/AutoSettingUI)
- [AOT Source Generator Documentation](https://github.com/StarryXYJ/AutoSettingUI/blob/main/manual/aot-source-generator.md)

## License

MIT
