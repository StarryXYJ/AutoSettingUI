# AutoSettingUI – Improvement Suggestions

This document lists actionable improvements for the AutoSettingUI project. Items are ordered by rough priority.

---

## High Priority

### 1. Source Generator: Full Coverage of Core Attributes
**File:** `src/AutoSettingUI.Generator/Incremental/AutoSettingGenerator.cs`

The current generator handles the most common attributes (`SettingUI`, `MainHeader`, `SubHeader`, `Hide`, `Title`, `Range`, `ItemsSource`, `ControlBinding`) but does not generate code for:
- Validating the generated class with a Roslyn diagnostic when `[SettingUI]` is applied to a non-public or nested class.
- Reporting as a Roslyn diagnostic when a `[Range]` attribute is placed on a non-numeric type.

### 2. WPF Panel: Adopt IPropertyValueAccessor
**File:** `src/Extensions/AutoSettingUI.WPF/Factories/WpfControlFactory.cs`

`WpfControlFactory.GetValue` / `WpfControlFactory.SetValue` still use `PropertyInfo.GetValue/SetValue` internally. These should accept an `IPropertyValueAccessor` passed in from `WpfAutoSettingPanel` so the WPF path can also benefit from AOT-generated accessors.

### 3. Replace `Type.GetType()` with `typeof()` in Generated Code
**File:** `src/AutoSettingUI.Generator/Incremental/AutoSettingGenerator.cs`

In the generated `GetValue`/`SetValue` switch blocks the enum type is looked up by string at runtime with `Type.GetType(prop.PropertyTypeName)` in the panels. The generator could instead emit `typeof(SomeEnum)` directly, removing any remaining runtime type string resolution.

---

## Medium Priority

### 4. Avalonia / Ursa: Use Data Bindings Instead of Event Subscriptions
**Files:** `AvaloniaAutoSettingPanel.cs`, `UrsaAutoSettingPanel.cs`

Controls currently subscribe to `TextChanged` / `SelectionChanged` / `IsCheckedChanged` events and call `_accessor.SetValue`. Using Avalonia's compiled data binding (`{Binding}` with `Source`) would be more idiomatic, reduce boilerplate, and play better with the MVVM pattern. However, this requires the target to implement `INotifyPropertyChanged`.

### 5. Lazy / Virtualized UI for Large Settings Classes
**Files:** Avalonia / Ursa / WPF panels

The entire settings form is built eagerly. For classes with many (50+) properties this can cause noticeable lag. Consider virtualizing the settings list or building sections on demand when navigated to.

### 6. `ItemsSourceAttribute`: Runtime Resolution is Reflection-Dependent
**Files:** All panels

When `[ItemsSource(typeof(MyClass), "MyStaticList")]` is used, the panels currently resolve the source by calling `Type.GetType(...)` and then use `PropertyInfo` or `MethodInfo`. The Source Generator could instead emit a direct static call to `MyClass.MyStaticList` in the generated code to be fully AOT-safe.

### 7. Navigation Scroll Accuracy
**Files:** `AvaloniaAutoSettingPanel.cs`, `UrsaAutoSettingPanel.cs`

`FindChildByTag` performs a full visual-tree walk every time navigation is clicked. Storing a `Dictionary<string, Control>` mapping section IDs to their header controls when the form is built would make scroll-to-section O(1) instead of O(n).

---

## Low Priority

### 8. NuGet Packaging
Add proper NuGet packaging metadata (`Description`, `ProjectUrl`, `RepositoryUrl`, `PackageTags`, `PackageReadmeFile`) to all library `.csproj` files so they are ready for `dotnet pack` publication.

### 9. Unit Tests
There are no automated tests in this solution. Consider adding:
- A unit test project for `AutoSettingUI.Core` logic (descriptor building, provider lookup).
- A Roslyn source-generator test project using `Microsoft.CodeAnalysis.CSharp.Testing`.

### 10. Demo Version Alignment
`AutoSettingUI.Avalonia.Demo` references Avalonia `11.2.7` while `AutoSettingUI.Avalonia` (the library) references `11.2.3`. These should be pinned to the same version to prevent subtle runtime issues.
