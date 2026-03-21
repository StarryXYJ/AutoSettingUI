# Usage Examples

---

## 1. Basic Settings Class

```csharp
using AutoSettingUI.Core.Attributes;

[SettingUI(Category = "General", Order = 0)]
[MainHeader("General Settings")]
public class GeneralSettings
{
    [Title("Application Name")]
    public string AppName { get; set; } = "My App";

    [Title("Enable Dark Mode")]
    public bool DarkMode { get; set; } = false;

    [Title("Language")]
    public string Language { get; set; } = "en-US";
}
```

---

## 2. Numeric Range (Slider)

```csharp
[SettingUI(Category = "Audio")]
public class AudioSettings
{
    [Title("Master Volume"), Range(0, 100)]
    public int Volume { get; set; } = 75;

    [Title("Balance"), Range(-1.0, 1.0)]
    public double Balance { get; set; } = 0.0;
}
```

---

## 3. Enum Property (ComboBox)

```csharp
public enum Theme { Light, Dark, System }

[SettingUI]
public class AppearanceSettings
{
    [Title("Theme")]
    public Theme SelectedTheme { get; set; } = Theme.System;
}
```

---

## 4. Sub-Sections

```csharp
[SettingUI]
[MainHeader("Network")]
public class NetworkSettings
{
    [Title("Host")] public string Host { get; set; } = "localhost";
    [Title("Port")] public int Port { get; set; } = 8080;

    [SubHeader("Proxy")]
    public bool UseProxy { get; set; } = false;
    [Title("Proxy Host")] public string ProxyHost { get; set; } = "";
    [Title("Proxy Port")] public int ProxyPort { get; set; } = 3128;
}
```

---

## 5. ItemsSource (Dynamic List)

```csharp
[SettingUI]
public class LocaleSettings
{
    public static IEnumerable<string> AvailableLanguages { get; } =
        new[] { "en-US", "zh-CN", "ja-JP", "de-DE" };

    [Title("Language")]
    [ItemsSource(typeof(LocaleSettings), nameof(AvailableLanguages))]
    public string Language { get; set; } = "en-US";
}
```

---

## 6. Hide Internal Properties

```csharp
[SettingUI]
public class PersistedSettings
{
    [Title("User Name")] public string UserName { get; set; } = "";

    [Hide] // not shown in the form
    public string SessionToken { get; set; } = "";
}
```

---

## 7. Multiple Settings Objects in One Panel

```csharp
// In ViewModel:
public IList<object> AllSettings { get; } = new List<object>
{
    new GeneralSettings(),
    new AudioSettings(),
    new NetworkSettings(),
};
```

```xml
<auto:AvaloniaAutoSettingPanel Targets="{Binding AllSettings}" />
```

Each class generates its own section in the form with its own navigation node.

---

## 8. AOT Setup (Avalonia)

```csharp
// In App.axaml.cs or wherever the panel is initialised:
var provider = new AutoSettingUI.Generated.GeneratedSettingProvider();
myPanel.DescriptorProvider = provider;
myPanel.PropertyAccessor   = provider;
```

The `GeneratedSettingProvider` is automatically created by the Roslyn Source Generator at compile time. No additional setup is required.

---

## 9. Custom Control Binding

```csharp
// A custom Avalonia slider control:
public class FancySlider : Slider
{
    public static readonly StyledProperty<double> FancyValueProperty =
        AvaloniaProperty.Register<FancySlider, double>(nameof(FancyValue));

    public double FancyValue
    {
        get => GetValue(FancyValueProperty);
        set => SetValue(FancyValueProperty, value);
    }
}

// Usage in settings class:
[Title("Volume")]
[ControlBinding(typeof(FancySlider), BindingProperty = "FancyValue")]
public double Volume { get; set; } = 0.5;
```

---

## 10. Localization (i18n)

AutoSettingUI supports dynamic language switching using resource keys.

### Step 1: Create Resource Files

Create `.resx` files for each language:

**Strings.resx** (default/English):
```xml
<data name="Settings.Theme" xml:space="preserve">
    <value>Theme Personalization</value>
</data>
<data name="Settings.Volume" xml:space="preserve">
    <value>Master Volume</value>
</data>
```

**Strings.zh-CN.resx** (Chinese):
```xml
<data name="Settings.Theme" xml:space="preserve">
    <value>主题个性化</value>
</data>
<data name="Settings.Volume" xml:space="preserve">
    <value>主音量</value>
</data>
```

### Step 2: Use Resource Keys in Attributes

```csharp
[SettingUI]
[MainHeader("Settings.Theme", UseResourceKey = true)]
public class ThemeSettings
{
    [Title("Settings.Volume", UseResourceKey = true)]
    [Range(0, 100)]
    public int Volume { get; set; } = 50;

    [SubHeader("Settings.Equalizer", UseResourceKey = true)]
    public bool EqEnabled { get; set; }
}
```

### Step 3: Setup LocalizationService

```csharp
// In ViewModel
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Services;

public class MainViewModel
{
    public ILocalizationService LocalizationService { get; }

    public MainViewModel()
    {
        LocalizationService = new ResxLocalizationService(Strings.ResourceManager);
    }

    public void SwitchToChinese() => LocalizationService.SetCulture("zh-CN");
    public void SwitchToEnglish() => LocalizationService.SetCulture("en");
}
```

### Step 4: Bind to Panel

```xml
<controls:UrsaAutoSettingPanel 
    Targets="{Binding Settings}"
    LocalizationService="{Binding LocalizationService}" />
```

When `SetCulture()` is called, all text elements (headers, labels, navigation nodes) update automatically.
