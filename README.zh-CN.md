# AutoSettingUI

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Core?label=Core)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Avalonia?label=Avalonia)](https://www.nuget.org/packages/AutoSettingUI.Avalonia/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Ursa?label=Ursa)](https://www.nuget.org/packages/AutoSettingUI.Ursa/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.WPF?label=WPF)](https://www.nuget.org/packages/AutoSettingUI.WPF/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Generator?label=Generator)](https://www.nuget.org/packages/AutoSettingUI.Generator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AutoSettingUI.Core?label=Downloads)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

**AutoSettingUI** 是一个 .NET 库，通过为普通 C# 对象标注特性（Attribute），自动生成设置界面面板。只需装饰好类，将实例传入控件，即可得到一个完整可交互的设置表单，无需手动布局任何 UI。

## 功能亮点

- 🎨 **多框架支持** — Avalonia、Ursa（Avalonia 主题）和 WPF
- ✨ **特性驱动渲染** — 使用特性灵活自定义每个属性的渲染方式
- ⚡ **AOT 兼容** — 增量 Roslyn 源生成器，完整支持 Native AOT 与裁剪发布
- 🔄 **MVVM 支持** — 完美支持 CommunityToolkit.Mvvm `[ObservableProperty]`
- 🌐 **国际化支持** — 内置本地化服务，支持动态语言切换，基于 [DynamicLocalization](https://github.com/StarryXYJ/Avalonia.DynamicLocalization)
- 🔌 **高度可扩展** — 可注入自定义 Provider 和 Accessor
- 🧭 **内置导航栏** — 侧边栏树形导航
- 🎯 **扩展控件支持** — ColorPicker、DatePicker、TimePicker、NumericUpDown 等

## 安装

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" />

<!-- Ursa (带 Ursa 主题的 Avalonia) -->
<PackageReference Include="AutoSettingUI.Ursa" />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF" />

<!-- AOT 支持（可选） -->
<PackageReference Include="AutoSettingUI.Generator" />
```

**目标框架：** `net8.0; net9.0; net10.0`（WPF: `net8.0-windows; net9.0-windows; net10.0-windows`）

**Avalonia：** `AutoSettingUI.Avalonia` 需要 Avalonia >= 11.0.0。`AutoSettingUI.Ursa` 需要 Avalonia >= 11.1.1（Ursa 依赖）。

## 快速上手

### 1. 添加样式引用（仅 Avalonia/Ursa）

> **⚠️ 重要：** 对于 Avalonia 和 Ursa 项目，必须在 `App.axaml` 中添加主题样式引用。

**Avalonia:**
```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
    <StyleInclude Source="avares://AutoSettingUI.Avalonia/Themes/Generic.axaml"/>
</Application.Styles>
```

**Ursa:**
```xml
<Application.Styles>
    <u-semi:SemiTheme Locale="zh-CN" />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
    <StyleInclude Source="avares://AutoSettingUI.Ursa/Themes/Generic.axaml"/>
</Application.Styles>
```

### 2. 定义设置模型

```csharp
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Avalonia.Attributes; // 或 AutoSettingUI.Ursa.Attributes

[SettingUI]
[MainHeader("应用程序设置")]
public class AppSettings
{
    [Title("应用程序名称")]
    public string AppName { get; set; } = "我的应用程序";

    [Title("音量")]
    [Range(0, 100)]
    public int Volume { get; set; } = 50;

    [Title("启用日志")]
    [CheckBox]
    public bool EnableLogging { get; set; }

    [Title("字体大小")]
    [NumericUpDown(Minimum = 8, Maximum = 32)]
    public int FontSize { get; set; } = 14;
}
```

#### 配合 CommunityToolkit.Mvvm 使用

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

[SettingUI]
[MainHeader("应用程序设置")]
public partial class ApplicationSettings : ObservableObject
{
    [Title("应用程序名称")]
    [ObservableProperty]
    private string _appName = "我的应用程序";

    [Title("启用日志")]
    [ObservableProperty]
    private bool _enableLogging;
}
```

### 3. 绑定面板

**Avalonia:**
```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">
    <auto:AvaloniaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**Ursa:**
```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**WPF:**
```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

**ViewModel:**
```csharp
public class MainViewModel
{
    public IEnumerable<object> Targets { get; } = new object[] { new AppSettings() };
}
```

### 4. AOT 支持（可选）

对于 Native AOT 或裁剪发布，生成器会自动使用。如需强制注册：

```csharp
using AutoSettingUI.Core.Registry;
using AutoSettingUI.Generated;

AotSettingRegistry.Provider = new GeneratedSettingProvider();
AotSettingRegistry.Accessor = AotSettingRegistry.Provider;
```

## 国际化（i18n）

AutoSettingUI 通过 [DynamicLocalization](https://github.com/StarryXYJ/DynamicLocalization) 支持动态语言切换。

### 使用资源键

在标题特性上设置 `UseResourceKey = true`：

```csharp
[SettingUI]
[MainHeader("Settings.Application", UseResourceKey = true)]
public class ApplicationSettings
{
    [Title("Settings.AppName", UseResourceKey = true)]
    public string AppName { get; set; } = "我的应用程序";

    [Title("Settings.EnableLogging", UseResourceKey = true)]
    public bool EnableLogging { get; set; }
}
```

### 配置本地化服务

**1. 创建资源文件（.resx）：**

- `Strings.resx`（默认/英文）
- `Strings.zh-CN.resx`（简体中文）

**2. 创建资源访问类：**

```csharp
namespace YourApp.Resources;

public static class Strings
{
    public static System.Resources.ResourceManager ResourceManager { get; } 
        = new System.Resources.ResourceManager(
            "YourApp.Resources.Strings", 
            typeof(Strings).Assembly);
}
```

**3. 配置本地化服务：**

```csharp
using DynamicLocalization.Core;
using DynamicLocalization.Core.Providers;

public class MainViewModel
{
    public ICultureService LocalizationService { get; }

    public MainViewModel()
    {
        var cultureService = new CultureService();
        var resxProvider = new ResxLocalizationProvider();
        resxProvider.Initialize(new ResxLocalizationProviderOptions
        {
            ResourceType = typeof(Strings)
        });
        cultureService.RegisterProvider(resxProvider);
        LocalizationService = cultureService;
    }

    public void SwitchToEnglish() => LocalizationService.SetCulture("en");
    public void SwitchToChinese() => LocalizationService.SetCulture("zh-CN");
}
```

**4. 绑定到面板：**

```xml
<auto:AvaloniaAutoSettingPanel 
    Targets="{Binding Targets}"
    LocalizationService="{Binding LocalizationService}" />
```

### 本地化数据源

DynamicLocalization 支持多种翻译数据源：

| 提供者 | 说明 |
|--------|------|
| `ResxLocalizationProvider` | 使用 .NET .resx 资源文件 |
| `JsonLocalizationProvider` | 基于 JSON 文件的翻译 |
| 自定义提供者 | 实现 `ILocalizationProvider` 接口，支持数据库、API 等 |

更多数据源和高级用法，请参阅 [DynamicLocalization](https://github.com/StarryXYJ/DynamicLocalization)。

## 可用特性一览

| 特性               | 目标   | 说明                       |
| ------------------ | ------ | -------------------------- |
| `[SettingUI]`      | 类     | 标记类以生成 UI            |
| `[MainHeader]`     | 类     | 设置分区标题               |
| `[Title]`          | 属性   | 设置属性标签               |
| `[SubHeader]`      | 属性   | 创建子分区                 |
| `[Hide]`           | 属性   | 从 UI 中隐藏               |
| `[Range]`          | 属性   | 数值范围（渲染为滑块）     |
| `[ItemsSource]`    | 属性   | 下拉框数据源               |
| `[ControlBinding]` | 属性   | 自定义控件绑定             |
| `[ReadOnly]`       | 属性   | 设置为只读                 |
| `[Password]`       | 属性   | 密码输入框                 |
| `[Placeholder]`    | 属性   | 输入框占位文本             |
| `[Layout]`         | 属性   | 自定义布局（宽度、高度）   |
| `[Validation]`     | 属性   | 自定义验证方法             |
| `[DisplayOrder]`   | 属性   | 控制显示顺序（数值小在前） |
| `[CollectionEditor]`| 属性  | 配置集合编辑，支持增删改查、排序及选中项绑定 |

## 扩展控件

| 特性              | 框架     | 说明               |
| ----------------- | -------- | ------------------ |
| `[CheckBox]`      | 所有     | 布尔属性复选框     |
| `[DatePicker]`    | 所有     | DateTime 日期选择  |
| `[TimePicker]`    | Avalonia/Ursa | TimeSpan 时间选择 |
| `[NumericUpDown]` | Avalonia/Ursa | 数值增减输入     |
| `[ColorPicker]`   | Avalonia/Ursa | 颜色选择         |
| `[TagInput]`      | Ursa     | 字符串集合标签输入 |
| `[IPv4Box]`       | Ursa     | IP 地址输入        |

> **注意：** ColorPicker 需要在 `App.axaml` 中添加 `<StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>`。

## 自定义控件绑定

`[ControlBinding]` 特性允许为属性指定自定义控件。有多种使用方式：

### 1. 使用 BindingProperty 自动绑定

指定控件类型和要绑定的属性：

```csharp
[ControlBinding(typeof(ProgressBar), "Value")]
public double ProgressValue { get; set; } = 50.0;
```

这会自动在 `ProgressValue` 和 `ProgressBar.Value` 之间创建双向绑定。

### 2. 工厂方法配合自定义绑定

当需要完全控制绑定（如自定义转换器、验证）时，使用工厂方法但不指定 `BindingProperty`：

```csharp
[ControlBinding(typeof(Slider), FactoryMethod = nameof(CreateCustomSlider))]
public double CustomSliderValue { get; set; } = 75.0;

public Slider CreateCustomSlider()
{
    var slider = new Slider { Minimum = 0, Maximum = 100, Width = 200 };
    
    // 自定义绑定逻辑
    slider.Bind(Slider.ValueProperty, 
        new Binding(nameof(CustomSliderValue)) 
        { 
            Source = this, 
            Mode = BindingMode.TwoWay 
        });
    
    return slider;
}
```

> **重要：** 使用工厂方法但不指定 `BindingProperty` 时，工厂方法需自行处理绑定，框架不会自动绑定。

### 3. 仅指定控件类型（自动检测）

仅指定控件类型，框架会尝试自动检测绑定属性：

```csharp
[ControlBinding(typeof(TextBox))]
public string AutoDetectedText { get; set; } = "自动检测绑定";
```

框架会尝试绑定到常见属性如 `Text`、`Value`、`IsChecked` 等。

### 4. 复杂工厂方法

创建完全自定义的控件，完全控制外观和行为：

```csharp
[ControlBinding(typeof(StackPanel), FactoryMethod = nameof(CreateRatingControl))]
[ObservableProperty]
private int _rating = 3;

public StackPanel CreateRatingControl()
{
    var panel = new StackPanel { Orientation = Orientation.Horizontal };
    
    for (int i = 1; i <= 5; i++)
    {
        var starIndex = i;
        var button = new Button { Content = "★", FontSize = 24 };
        
        button.Click += (s, e) => Rating = starIndex;
        button.Bind(Button.ForegroundProperty,
            new Binding(nameof(Rating))
            {
                Source = this,
                Converter = new RatingConverter(starIndex)
            });
        
        panel.Children.Add(button);
    }
    
    return panel;
}
```

### 绑定行为总结

| BindingProperty | FactoryMethod | 行为 |
|-----------------|---------------|------|
| 已指定 | 未指定 | 自动绑定到指定属性 |
| 已指定 | 已指定 | 自动绑定到指定属性（工厂创建控件） |
| 未指定 | 未指定 | 自动检测常见属性（仅 WPF） |
| 未指定 | 已指定 | **不自动绑定** — 工厂方法处理绑定 |

### 创建自定义控件特性

通过继承 `ControlBindingAttribute` 创建可复用的特性：

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") { }
}

// 使用
[DatePicker]
public DateTime BirthDate { get; set; }
```

带参数的复杂控件：

```csharp
public sealed class NumericUpDownAttribute : ControlBindingAttribute
{
    public double Minimum { get; set; }
    public double Maximum { get; set; } = 100;

    public NumericUpDownAttribute() 
        : base(typeof(NumericUpDown), "Value", nameof(CreateControl)) { }

    public Control CreateControl(Type propertyType) => new NumericUpDown
    {
        Minimum = (decimal)Minimum,
        Maximum = (decimal)Maximum
    };
}

// 使用
[NumericUpDown(Minimum = 0, Maximum = 100)]
public int ItemCount { get; set; }
```

## 集合编辑

AutoSettingUI 提供内置的集合编辑支持，包括添加、删除和排序功能。还可以将选中项绑定到单独的属性进行详细编辑。

### 基本集合编辑

```csharp
[SettingUI]
public class Settings
{
    [Title("标签")]
    public ObservableCollection<string> Tags { get; set; } = ["重要", "工作"];

    [Title("人员")]
    public ObservableCollection<Person> People { get; set; } = new();
}
```

### 选中项绑定

使用 `SelectedItemProperty` 将集合的选中项绑定到属性，实现对选中项的详细编辑：

```csharp
[SettingUI]
public class Settings
{
    [Title("标签")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedTag))]
    public ObservableCollection<string> Tags { get; set; } = ["重要", "工作"];

    [Title("选中的标签")]
    [Description("上方列表中当前选中的标签")]
    public string? SelectedTag { get; set; }

    [Title("人员")]
    [CollectionEditor(SelectedItemProperty = nameof(SelectedPerson))]
    public ObservableCollection<Person> People { get; set; } = new();

    [Title("选中的人员")]
    [Description("在下方编辑选中人员的详细信息")]
    public Person? SelectedPerson { get; set; }
}
```

当用户在集合编辑器中选择项目时，`SelectedTag` 或 `SelectedPerson` 属性会自动更新。通过代码修改这些属性时，UI 也会同步反映变化。

### 集合编辑器选项

| 属性 | 类型 | 说明 |
|------|------|------|
| `AllowAdd` | `bool` | 允许添加新项目（默认：`true`） |
| `AllowRemove` | `bool` | 允许删除项目（默认：`true`） |
| `AllowReorder` | `bool` | 允许重新排序（默认：`true`） |
| `AllowEditItems` | `bool` | 允许行内编辑项目（默认：`true`） |
| `SelectedItemProperty` | `string` | 绑定选中项的属性名 |
| `EditorTypeName` | `string` | 自定义编辑器类型名 |
| `FactoryMethod` | `string` | 自定义编辑器的工厂方法 |

### 只读集合

```csharp
[Title("版本列表（只读）")]
[CollectionEditor(AllowAdd = false, AllowRemove = false, AllowReorder = false)]
public ObservableCollection<string> Versions { get; set; } = ["1.0.0", "1.1.0", "2.0.0"];
```

## 演示应用

仓库包含展示所有功能的演示应用：

| 演示 | 框架 | 说明 |
|------|------|------|
| `AutoSettingUI.Avalonia.CrossPlatform.Demo` | Avalonia | 跨平台（桌面、Android、iOS、浏览器） |
| `AutoSettingUI.Ursa.Demo` | Ursa | Ursa 主题 Avalonia，含扩展控件 |
| `AutoSettingUI.Wpf.Demo` | WPF | Windows Presentation Foundation |

### 演示功能

- ✅ 动态语言切换（英文/中文）
- ✅ 主题切换（浅色/深色/系统）
- ✅ 导航栏切换
- ✅ 自定义样式面板
- ✅ 扩展控件演示
- ✅ 集合编辑与选中项绑定

## 包说明

| 包名                      | 说明                     | 依赖                  |
| ------------------------- | ------------------------ | --------------------- |
| `AutoSettingUI.Avalonia`  | Avalonia UI 面板         | Core, Extension.Shared |
| `AutoSettingUI.Ursa`      | Ursa 主题 Avalonia 面板  | Core, Extension.Shared |
| `AutoSettingUI.WPF`       | WPF 面板                 | Core, Extension.Shared |
| `AutoSettingUI.Generator` | Roslyn 源生成器（AOT）   | 独立（Analyzer）      |
| `AutoSettingUI.Core`      | 特性、接口、模型         | DynamicLocalization.Core |

> **注意：** 安装 UI 框架包时会自动引入 `Core` 和 `Extension.Shared`。

## 文档

- [架构说明](manual/architecture.md) — 项目结构与数据流
- [特性参考](manual/attributes.md) — 所有可用特性
- [框架扩展](manual/extensions.md) — 各框架特定用法
- [AOT 源生成器](manual/aot-source-generator.md) — AOT 支持详情

## 构建

```powershell
# Debug 构建
.\build-all.ps1

# Release 构建并打包
.\build-all.ps1 -Configuration Release -Pack

# 发布演示应用
.\publish-demo.ps1 -Framework Avalonia -Mode AOT
.\publish-demo.ps1 -Framework Ursa -Mode Normal
.\publish-demo.ps1 -Framework WPF -Mode Normal
```

## 许可证

MIT
