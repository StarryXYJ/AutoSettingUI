# AutoSettingUI.Extension.Shared

Shared utilities and helpers for **AutoSettingUI** framework extensions. This package contains common code used by all UI framework implementations (Avalonia, Ursa, WPF).

[![NuGet](https://img.shields.io/nuget/v/ASettingUI.Extension.Shared.svg)](https://www.nuget.org/packages/AutoSettingUI.Extension.Shared/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Installation

```xml
<PackageReference Include="AutoSettingUI.Extension.Shared" Version="1.0.0" />
```

> **Note:** This package is typically installed automatically as a dependency of the framework-specific packages.

## Features

### ValidationHelper

Provides unified validation logic for property values:

```csharp
using AutoSettingUI.Extension.Shared;

// Validate a property value
bool isValid = ValidationHelper.ValidateValue(propertyInfo, value, target, out string? errorMessage);
```

Supported validations:
- Required field validation
- String length validation (MinLength/MaxLength)
- Pattern matching (Regex)
- Numeric range validation (MinValue/MaxValue)
- Custom validation method invocation

### ExtendedControlHelper

Simplifies creating controls from `ControlBindingAttribute`:

```csharp
using AutoSettingUI.Extension.Shared;

// Create a control from attribute
var control = ExtendedControlHelper.CreateControl(attribute, propertyType);
```

Features:
- Factory method invocation (static and instance methods)
- Control instantiation via reflection
- Binding property resolution

### ControlFactoryBase

Base class for control factories providing common functionality:

```csharp
public class MyControlFactory : ControlFactoryBase
{
    // Inherits:
    // - Property value access via IPropertyValueAccessor
    // - Dynamic read-only state evaluation
    // - CanExecute delegate evaluation
    // - Type resolution from name strings
}
```

## Usage

This package is primarily used internally by the framework extensions. If you're creating a custom extension, you can reference it directly:

```csharp
using AutoSettingUI.Extension.Shared;

public class MyCustomPanel
{
    protected bool ValidateProperty(PropertyInfo prop, object? value, object target)
    {
        return ValidationHelper.ValidateValue(prop, value, target, out _);
    }

    protected object? CreateExtendedControl(ControlBindingAttribute attr, Type propertyType)
    {
        return ExtendedControlHelper.CreateControl(attr, propertyType);
    }
}
```

## Related Packages

- **AutoSettingUI.Core** - Core attributes and interfaces
- **AutoSettingUI.Generator** - Roslyn source generator
- **AutoSettingUI.Avalonia** - Avalonia UI panel
- **AutoSettingUI.Ursa** - Ursa-themed Avalonia panel
- **AutoSettingUI.WPF** - WPF panel

## Documentation

- [GitHub Repository](https://github.com/StarryXYJ/AutoSettingUI)
- [Architecture Documentation](https://github.com/StarryXYJ/AutoSettingUI/blob/main/manual/architecture.md)

## License

MIT
