# Attributes Reference

All attributes live in the `AutoSettingUI.Core.Attributes` namespace.

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

| Parameter      | Type     | Description                 |
| -------------- | -------- | --------------------------- |
| `title` (ctor) | `string` | The header text to display. |

```csharp
[SettingUI]
[MainHeader("Sound & Audio")]
public class AudioSettings { ... }
```

---

## `[SubHeader]`

Applied to a **property** to begin a new sub-section. All subsequent properties are grouped under this sub-section until the next `[SubHeader]` or end of class.

**Target:** `Property`

| Parameter      | Type      | Description               |
| -------------- | --------- | ------------------------- |
| `title` (ctor) | `string`  | The sub-section title.    |
| `Icon`         | `string?` | Optional icon identifier. |

```csharp
[SubHeader("Equalizer")]
public bool EqEnabled { get; set; }
```

---

## `[Title]`

Sets the human-readable label displayed next to a property in the form.

**Target:** `Property`

| Parameter     | Type     | Description                      |
| ------------- | -------- | -------------------------------- |
| `name` (ctor) | `string` | Display label for this property. |

```csharp
[Title("Master Volume")]
public int Volume { get; set; }
```

---

## `[Hide]`

Excludes a property from being rendered in the settings form.

**Target:** `Property`  
_(No parameters)_

```csharp
[Hide]
public string InternalId { get; set; } = Guid.NewGuid().ToString();
```

---

## `[Range]`

Specifies minimum and maximum bounds for a numeric property. The panel will render a slider when this attribute is present.

**Target:** `Property`

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

**Target:** `Property`

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

**Target:** `Property`

| Parameter / Property | Type      | Description                                                                            |
| -------------------- | --------- | -------------------------------------------------------------------------------------- |
| `controlType` (ctor) | `Type`    | The custom control type.                                                               |
| `BindingProperty`    | `string?` | The target dependency/styled property name on the custom control to bind the value to. |
| `FactoryMethod`      | `string?` | Static method name on `controlType` to create the control instance.                    |

```csharp
[ControlBinding(typeof(ColorPicker), BindingProperty = "Color")]
public Color AccentColor { get; set; }
```
