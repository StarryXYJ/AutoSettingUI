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

AutoSettingUI.Avalonia
    ├── AutoSettingUI.Core
    └── AutoSettingUI.Generator [Analyzer]

AutoSettingUI.Ursa
    ├── AutoSettingUI.Core
    └── AutoSettingUI.Generator [Analyzer]

AutoSettingUI.WPF
    ├── AutoSettingUI.Core
    └── AutoSettingUI.Generator [Analyzer]
```

## Data Flow

1. **Compile time** — The Roslyn Source Generator scans for classes with `[SettingUI]` and emits `GeneratedSettingProvider.g.cs` into the consuming project.
2. **Startup** — The panel control calls `InitializeProvider()`. If no `DescriptorProvider` is injected, it falls back to the reflection-based `ReflectionSettingDescriptorProvider`.
3. **Rendering** — `BuildForm()` walks all `Targets`, calls `GetDescriptor(targetType)` to get a `SettingClassDescriptor`, then constructs UI controls for each `PropertyDescriptor`.
4. **Reading/writing values** — `IPropertyValueAccessor.GetValue/SetValue` is called for every property interaction (no `PropertyInfo` involved in the AOT-generated path).
