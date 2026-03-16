# Architecture

## Project Layout

```
AutoSettingUI/
├── src/
│   ├── AutoSettingUI.Core/          # Attributes, interfaces, models — no UI dependency
│   │   ├── Attributes/              # [SettingUI], [Title], [Range], [Hide], [SubHeader],
│   │   │                            #   [ItemsSource], [ControlBinding], [MainHeader]
│   │   ├── Interfaces/              # ISettingDescriptorProvider, IPropertyValueAccessor,
│   │   │                            #   ReflectionPropertyAccessor
│   │   ├── Models/                  # SettingClassDescriptor, PropertyDescriptor,
│   │   │                            #   SubSectionInfo, NavigationNode
│   │   └── Providers/               # ReflectionSettingDescriptorProvider (non-AOT fallback)
│   │
│   ├── AutoSettingUI.Generator/     # Roslyn Incremental Source Generator
│   │   └── Incremental/
│   │       └── AutoSettingGenerator.cs
│   │
│   └── Extensions/
│       ├── AutoSettingUI.Avalonia/  # AvaloniaAutoSettingPanel (plain Avalonia)
│       ├── AutoSettingUI.Ursa/      # UrsaAutoSettingPanel (Ursa-themed Avalonia)
│       └── AutoSettingUI.WPF/       # WpfAutoSettingPanel + WpfControlFactory
│
├── AutoSettingUI.Extension.Shared/  # Shared code between all UI frameworks
│   ├── ControlFactoryBase.cs        # Base class for control factories
│   ├── DelegateCommandBase.cs       # Base class for delegate commands
│   ├── ValidationHelper.cs          # Shared validation logic
│   └── ExtendedControlHelper.cs     # Helper for ControlBindingAttribute
│
└── Demo/
    ├── AutoSettingUI.Avalonia.Demo/
    ├── AutoSettingUI.Ursa.Demo/
    └── AutoSettingUI.Wpf.Demo/
```

## Dependency Graph

```
AutoSettingUI.Generator (netstandard2.0, Analyzer)
    └── purely compile-time, produces GeneratedSettingProvider.g.cs

AutoSettingUI.Core (net9.0, AOT-compatible)
    └── (no dependencies)

AutoSettingUI.Extension.Shared (net9.0, AOT-compatible)
    └── AutoSettingUI.Core

AutoSettingUI.Avalonia
    ├── AutoSettingUI.Core
    ├── AutoSettingUI.Extension.Shared
    └── AutoSettingUI.Generator [Analyzer]

AutoSettingUI.Ursa
    ├── AutoSettingUI.Core
    ├── AutoSettingUI.Extension.Shared
    └── AutoSettingUI.Generator [Analyzer]

AutoSettingUI.WPF
    ├── AutoSettingUI.Core
    ├── AutoSettingUI.Extension.Shared
    └── AutoSettingUI.Generator [Analyzer]
```

## Shared Components

The `AutoSettingUI.Extension.Shared` project contains code shared across all UI frameworks:

### ValidationHelper
Provides unified validation logic for property values:
- Required field validation
- String length validation (MinLength/MaxLength)
- Pattern matching (Regex)
- Numeric range validation (MinValue/MaxValue)
- Custom validation method invocation

### ExtendedControlHelper
Simplifies creating controls from `ControlBindingAttribute`:
- Factory method invocation (static and instance methods)
- Control instantiation via reflection
- Binding property resolution

### ControlFactoryBase
Base class providing common functionality:
- Property value access via `IPropertyValueAccessor`
- Dynamic read-only state evaluation
- CanExecute delegate evaluation
- Type resolution from name strings

## Data Flow

1. **Compile time** — The Roslyn Source Generator scans for classes with `[SettingUI]` and emits `GeneratedSettingProvider.g.cs` into the consuming project.
2. **Startup** — The panel control calls `InitializeProvider()`. If no `DescriptorProvider` is injected, it falls back to the reflection-based `ReflectionSettingDescriptorProvider`.
3. **Rendering** — `BuildForm()` walks all `Targets`, calls `GetDescriptor(targetType)` to get a `SettingClassDescriptor`, then constructs UI controls for each `PropertyDescriptor`.
4. **Reading/writing values** — `IPropertyValueAccessor.GetValue/SetValue` is called for every property interaction (no `PropertyInfo` involved in the AOT-generated path).
