# Attributes Reference

All attributes live in the `AutoSettingUI.Core.Attributes` namespace.

---

## Target Support

Most property-level attributes support both **properties** and **fields**:

| Target | Supported |
|--------|-----------|
| `Property` | ✅ Always supported |
| `Field` (with `[ObservableProperty]`) | ✅ Supported in Source Generator mode |
| `Field` (plain) | ⚠️ Limited support |

> **Note:** When using `[ObservableProperty]` from CommunityToolkit.Mvvm, attributes can be applied directly to the field. The Source Generator will correctly process them.

```csharp
// Both approaches work with Source Generator
public partial class Settings
{
    // Traditional property
    [Title("Volume")]
    [Range(0, 100)]
    public int Volume { get; set; } = 50;

    // ObservableProperty field (CommunityToolkit.Mvvm)
    [ObservableProperty]
    [Title("Brightness")]
    [Range(0, 100)]
    private int _brightness = 50;
}
```

---

## `[SettingUI]`

Marks a class (or struct) to be automatically rendered as a settings form.

**Target:** `Class | Struct`

| Property         | Type      | Default | Description                                                 |
| ---------------- | --------- | ------- | ----------------------------------------------------------- |
| `Category`       | `string?` | `null`  | Groups this class under a named category in navigation.     |
| `Order`          | `int`     | `0`     | Display order among all settings classes (lower = first).   |
| `Icon`           | `string?` | `null`  | Framework-specific icon identifier for the navigation node. |
| `ControlFactory` | `Type?`   | `null`  | Default factory type used for all properties in this class. |
| `FactoryMethod`  | `string?` | `null`  | Static method name on `ControlFactory` to create controls.  |

```csharp
[SettingUI(Category = "Audio", Order = 1)]
public class AudioSettings { ... }
```

---

## `[MainHeader]`

Overrides the display title shown at the top of this class's section.

**Target:** `Class`

| Parameter / Property | Type      | Description                                    |
| -------------------- | --------- | ---------------------------------------------- |
| `title` (ctor)       | `string`  | The header text to display.                    |
| `Icon`               | `string?` | Optional icon identifier.                      |
| `Order`              | `int`     | Display order of this header.                  |
| `UseResourceKey`     | `bool`    | When `true`, `Title` is treated as a resource key. |

```csharp
[SettingUI]
[MainHeader("Sound & Audio")]
public class AudioSettings { ... }

// With localization
[SettingUI]
[MainHeader("Settings.Audio", UseResourceKey = true)]
public class AudioSettings { ... }
```

---

## `[SubHeader]`

Applied to a property to begin a new sub-section. All subsequent properties are grouped under this sub-section until the next `[SubHeader]` or end of class.

**Target:** `Property | Field`

| Parameter / Property | Type      | Description                                    |
| -------------------- | --------- | ---------------------------------------------- |
| `title` (ctor)       | `string`  | The sub-section title.                         |
| `Icon`               | `string?` | Optional icon identifier.                      |
| `UseResourceKey`     | `bool`    | When `true`, `Title` is treated as a resource key. |

```csharp
[SubHeader("Equalizer")]
public bool EqEnabled { get; set; }

// With localization
[SubHeader("Settings.Equalizer", UseResourceKey = true)]
public bool EqEnabled { get; set; }
```

---

## `[Title]`

Sets the human-readable label displayed next to a property in the form.

**Target:** `Property | Field`

| Parameter / Property | Type      | Description                                         |
| -------------------- | --------- | --------------------------------------------------- |
| `name` (ctor)        | `string`  | Display label for this property.                    |
| `Description`        | `string?` | Optional description/tooltip for the property.      |
| `UseResourceKey`     | `bool`    | When `true`, `Name` is treated as a resource key.   |
| `UseDescriptionKey`  | `bool`    | When `true`, `Description` is treated as a resource key. |

```csharp
[Title("Master Volume")]
public int Volume { get; set; }

// With localization
[Title("Settings.Volume", UseResourceKey = true)]
public int Volume { get; set; }
```

---

## `[Hide]`

Excludes a property from being rendered in the settings form.

**Target:** `Property | Field`

```csharp
[Hide]
public string InternalId { get; set; } = Guid.NewGuid().ToString();
```

---

## `[VisibleIf]`

Conditionally shows or hides a property based on a method's return value.

**Target:** `Property | Field`

| Parameter         | Type     | Description                                                              |
| ----------------- | -------- | ------------------------------------------------------------------------ |
| `methodName` (ctor) | `string` | Name of the method that returns a boolean indicating visibility.       |

The method must be parameterless and return `bool`. When the method returns `true`, the property is visible; when `false`, it is hidden.

```csharp
[SettingUI]
public class ProxySettings
{
    [Title("Use Proxy")]
    public bool UseProxy { get; set; }

    [Title("Proxy Address")]
    [VisibleIf(nameof(ShouldShowProxySettings))]
    public string ProxyAddress { get; set; } = "";

    [Title("Proxy Port")]
    [VisibleIf(nameof(ShouldShowProxySettings))]
    public int ProxyPort { get; set; } = 8080;

    // This method controls the visibility of ProxyAddress and ProxyPort
    public bool ShouldShowProxySettings() => UseProxy;
}
```

> **Note:** The visibility is updated automatically when any property in the settings class changes. Make sure your settings class implements `INotifyPropertyChanged` (or uses `[ObservableProperty]` from CommunityToolkit.Mvvm) for real-time updates.

---

## `[Range]`

Specifies minimum and maximum bounds for a numeric property. The panel will render a slider when this attribute is present.

**Target:** `Property | Field`

| Parameter        | Type     | Description                                                                |
| ---------------- | -------- | -------------------------------------------------------------------------- |
| `minimum` (ctor) | `double` | Minimum value.                                                             |
| `maximum` (ctor) | `double` | Maximum value.                                                             |
| `Step`           | `double` | Increment step (informational; slider uses default unless explicitly set). |

```csharp
[Title("Volume"), Range(0, 100)]
public int Volume { get; set; } = 50;
```

---

## `[ItemsSource]`

Points to a static property or method on a specified type that provides the list of selectable items for a dropdown.

**Target:** `Property | Field`

| Parameter                   | Type     | Description                                            |
| --------------------------- | -------- | ------------------------------------------------------ |
| `sourceType` (ctor)         | `Type`   | The type containing the source member.                 |
| `sourcePropertyName` (ctor) | `string` | Name of the static property or method on `sourceType`. |

```csharp
public static IEnumerable<string> Languages { get; } = new[] { "en-US", "zh-CN", "ja-JP" };

[ItemsSource(typeof(AppSettings), nameof(Languages))]
public string Language { get; set; } = "en-US";
```

---

## `[ControlBinding]`

Specifies a custom control type to use for rendering this specific property.

**Target:** `Property | Field`

| Parameter / Property | Type      | Description                                                                            |
| -------------------- | --------- | -------------------------------------------------------------------------------------- |
| `controlType` (ctor) | `Type`    | The custom control type.                                                               |
| `BindingProperty`    | `string?` | The target dependency/styled property name on the custom control to bind the value to. |
| `FactoryMethod`      | `string?` | Static method name on `controlType` to create the control instance.                    |

```csharp
[ControlBinding(typeof(ColorPicker), BindingProperty = "Color")]
public Color AccentColor { get; set; }
```

---

## `[DisplayOrder]`

Controls the display order of properties within a settings class. Properties with lower order values are displayed first. Properties with the same order value are displayed in their declaration order.

**Target:** `Property | Field`

| Parameter      | Type  | Default | Description                                    |
| -------------- | ----- | ------- | ---------------------------------------------- |
| `order` (ctor) | `int` | `0`     | Display order value. Lower values appear first. |

```csharp
[SettingUI]
public class AppSettings
{
    // Displayed third (order = 2)
    [DisplayOrder(2)]
    [Title("Advanced Options")]
    public bool EnableAdvanced { get; set; }

    // Displayed first (order = 0, default)
    [Title("Application Name")]
    public string AppName { get; set; }

    // Displayed second (order = 1)
    [DisplayOrder(1)]
    [Title("Version")]
    public string Version { get; set; }
}
```

---

## `[ReadOnly]`

Marks a property as read-only in the settings form. The value is displayed but cannot be edited.

**Target:** `Property | Field`

```csharp
[ReadOnly]
[Title("Installation Path")]
public string InstallPath { get; set; } = @"C:\Program Files\MyApp";
```

---

## `[Password]`

Marks a string property to be rendered as a password input field (masked characters).

**Target:** `Property | Field`

```csharp
[Password]
[Title("API Key")]
public string ApiKey { get; set; } = "";
```

---

## `[Placeholder]`

Sets placeholder text for input controls (e.g., TextBox watermarks).

**Target:** `Property | Field`

| Parameter / Property | Type      | Description                                    |
| -------------------- | --------- | ---------------------------------------------- |
| `text` (ctor)        | `string`  | Placeholder text to display.                   |
| `UseResourceKey`     | `bool`    | When `true`, `text` is treated as a resource key. |

```csharp
[Placeholder("Enter your name...")]
public string UserName { get; set; } = "";

// With localization
[Placeholder("Settings.UserNamePlaceholder", UseResourceKey = true)]
public string UserName { get; set; } = "";
```

---

## `[Description]`

Adds a description/tooltip to a property.

**Target:** `Property | Field`

| Parameter / Property | Type      | Description                                    |
| -------------------- | --------- | ---------------------------------------------- |
| `text` (ctor)        | `string`  | Description text.                              |
| `UseResourceKey`     | `bool`    | When `true`, `text` is treated as a resource key. |

```csharp
[Description("The primary color used for UI accents")]
[Title("Accent Color")]
public Color AccentColor { get; set; }
```

---

## `[Validation]`

Adds validation rules to a property. Can be applied multiple times for multiple rules.

**Target:** `Property | Field`

| Parameter       | Type     | Description                              |
| --------------- | -------- | ---------------------------------------- |
| `pattern` (ctor)| `string` | Regex pattern for validation.            |
| `message` (ctor)| `string` | Error message when validation fails.     |

```csharp
[Validation(@"^[a-zA-Z0-9_]+$", "Only alphanumeric characters and underscores allowed")]
[Title("Username")]
public string Username { get; set; } = "";
```

---

## `[CollectionEditor]`

Configures the collection editor for collection properties.

**Target:** `Property | Field`

| Property         | Type      | Description                                    |
| ---------------- | --------- | ---------------------------------------------- |
| `AllowAdd`       | `bool`    | Allow adding new items.                        |
| `AllowRemove`    | `bool`    | Allow removing items.                          |
| `AllowEdit`      | `bool`    | Allow editing items.                           |

```csharp
[CollectionEditor(AllowAdd = true, AllowRemove = true)]
[Title("Bookmarks")]
public ObservableCollection<string> Bookmarks { get; set; } = new();
```

---

## `[CommandCanExecute]`

Links a property's value to control the enabled state of a command button.

**Target:** `Property | Field`

| Parameter              | Type     | Description                                    |
| ---------------------- | -------- | ---------------------------------------------- |
| `commandName` (ctor)   | `string` | Name of the command property to control.       |

```csharp
[CommandCanExecute(nameof(SaveCommand))]
public bool HasUnsavedChanges { get; set; }

public ICommand SaveCommand { get; }
```

---

## `[Layout]`

Controls layout options for a property's control.

**Target:** `Property | Field`

| Property      | Type      | Description                                    |
| ------------- | --------- | ---------------------------------------------- |
| `Width`       | `double?` | Explicit width for the control.                |
| `Height`      | `double?` | Explicit height for the control.               |
| `HorizontalAlignment` | `string?` | Horizontal alignment (Left, Center, Right, Stretch). |

```csharp
[Layout(Width = 200, HorizontalAlignment = "Center")]
[Title("Search")]
public string SearchQuery { get; set; } = "";
```
