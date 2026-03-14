# Framework Extension Guide

This page documents how to use AutoSettingUI in each supported framework.

---

## Avalonia (`AutoSettingUI.Avalonia`)

### NuGet / Project Reference

```xml
<ProjectReference Include="path\to\AutoSettingUI.Avalonia.csproj" />
```

### XAML Usage

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">

    <auto:AvaloniaAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        ShowNavigation="True"
        NavigationWidth="200" />

</Window>
```

### Styled Properties

| Property             | Type                          | Default | Description                               |
| -------------------- | ----------------------------- | ------- | ----------------------------------------- |
| `Title`              | `string?`                     | `null`  | Optional panel-level title.               |
| `Targets`            | `IEnumerable?`                | `null`  | Collection of settings objects to render. |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null`  | Inject the generated provider for AOT.    |
| `PropertyAccessor`   | `IPropertyValueAccessor?`     | `null`  | Inject the generated accessor for AOT.    |
| `ShowNavigation`     | `bool`                        | `true`  | Show/hide the left sidebar navigation.    |
| `NavigationWidth`    | `double`                      | `200`   | Width of the navigation sidebar.          |

---

## Ursa (`AutoSettingUI.Ursa`)

Identical API to the Avalonia panel but styled with Ursa's card-based theme.

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">

    <ursa:UrsaAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        UseCardBorderTheme="True" />

</Window>
```

### Additional Styled Properties

| Property             | Type   | Default | Description                                |
| -------------------- | ------ | ------- | ------------------------------------------ |
| `UseCardBorderTheme` | `bool` | `true`  | Renders sections inside Ursa card borders. |

---

## WPF (`AutoSettingUI.WPF`)

### XAML Usage

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">

    <auto:WpfAutoSettingPanel
        Title="Settings"
        Targets="{Binding MySettingsList}"
        ShowNavigation="True"
        NavigationWidth="200" />

</Window>
```

### Dependency Properties

| Property             | Type                          | Default | Description                         |
| -------------------- | ----------------------------- | ------- | ----------------------------------- |
| `Title`              | `string?`                     | `null`  | Optional panel-level title.         |
| `Targets`            | `IList?`                      | `null`  | List of settings objects to render. |
| `DescriptorProvider` | `ISettingDescriptorProvider?` | `null`  | Custom provider.                    |
| `ShowNavigation`     | `bool`                        | `true`  | Show/hide sidebar.                  |
| `NavigationWidth`    | `double`                      | `200`   | Sidebar width.                      |

### WpfControlFactory

The WPF panel delegates control creation to `WpfControlFactory`. It automatically selects:

- `Slider` for `[Range]` numeric properties.
- `CheckBox` for `bool` properties.
- `ComboBox` for `enum` properties.
- `PasswordBox` for properties whose names contain `password` or `pwd`.
- `TextBox` (multi-line) for properties named `description`, `notes`, or `comment`.
- `TextBox` (single-line) for all other strings and types.
- Custom controls via `[ControlBinding]`.
