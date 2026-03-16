# AOT & Source Generator Guide

`AutoSettingUI.Generator` is a **Roslyn Incremental Source Generator** that makes AutoSettingUI fully compatible with .NET Native AOT and IL trimming by eliminating all runtime reflection for descriptor lookup and property access.

---

## How It Works

At build time, the generator:

1. Finds every class in your project decorated with `[SettingUI]`.
2. For each class, it generates:
   - A `Register_<ClassName>()` method that builds a `SettingClassDescriptor` with literal values.
   - A `case "PropertyName": return instance.PropertyName;` branch in `GetValue`.
   - A `case "PropertyName": instance.PropertyName = (T)value!;` branch in `SetValue`.
3. Emits a single file `GeneratedSettingProvider.g.cs` into your project.

The generated class implements **both** `ISettingDescriptorProvider` and `IPropertyValueAccessor`.

---

## Referencing the Generator

The generator is already wired in the `AutoSettingUI.Avalonia`, `AutoSettingUI.Ursa`, and `AutoSettingUI.WPF` project files:

```xml
<ProjectReference
    Include="..\AutoSettingUI.Generator\AutoSettingUI.Generator.csproj"
    OutputItemType="Analyzer"
    ReferenceOutputAssembly="false" />
```

In your consuming application project you need to add the same reference to your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\AutoSettingUI.Generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

---

## Enabling AOT in Your App

### 1. Inject the Generated Provider

Both `DescriptorProvider` and `PropertyAccessor` should point to the same `GeneratedSettingProvider` instance:

```csharp
var provider = new AutoSettingUI.Generated.GeneratedSettingProvider();

// Avalonia / Ursa
panel.DescriptorProvider = provider;
panel.PropertyAccessor   = provider;

// WPF
panel.DescriptorProvider = provider;
// WpfControlFactory accepts IPropertyValueAccessor in future improvement (see improvements.md)
```

### 2. Set AOT flags in your App's `.csproj`

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <PublishTrimmed>true</PublishTrimmed>
</PropertyGroup>
```

---

## What Requires Reflection Still

Even when using `GeneratedSettingProvider`, the following paths still use reflection and are not AOT-safe:

| Feature                               | Where                        | Notes                                                                                                 |
| ------------------------------------- | ---------------------------- | ----------------------------------------------------------------------------------------------------- |
| `[ControlBinding]` custom controls    | Avalonia / Ursa / WPF panels | `Type.GetType()`, `GetMethod()`, `GetField()` are still used to instantiate and bind custom controls. |
| `[ItemsSource]` resolution            | All panels                   | The items source is resolved via `Type.GetType()` + member lookup.                                    |
| `WpfControlFactory.GetValue/SetValue` | `AutoSettingUI.WPF`          | Uses `PropertyInfo`. See [improvements.md](../improvements.md).                                       |

If your AOT scenario does not use custom control bindings or runtime items sources, the generated provider is sufficient for full AOT compilation.

---

## Inspecting the Generated Code

To view the generated file, enable emitting source generator output in your MSBuild properties:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

The file will appear at:
```
obj/Debug/net9.0/Generated/AutoSettingUI.Generator/AutoSettingUI.Generator.Incremental.AutoSettingGenerator/GeneratedSettingProvider.g.cs
```
