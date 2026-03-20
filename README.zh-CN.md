# AutoSettingUI

**AutoSettingUI** 是一个 .NET 库，通过为普通 C# 对象标注特性（Attribute），自动生成设置界面面板。只需装饰好类，将实例传入控件，即可得到一个完整可交互的设置表单，无需手动布局任何 UI。

## 功能亮点

- 🎨 **多框架支持** — 包含 Avalonia、Ursa（Avalonia 主题）和 WPF 三种实现。
- ✨ **特性驱动渲染** — 使用 `[Title]`、`[Range]`、`[Hide]`、`[SubHeader]`、`[ItemsSource]`、`[ControlBinding]` 等特性灵活自定义每个属性的渲染方式。
- ⚡ **AOT 兼容** — 内置 Roslyn 增量源生成器（`AutoSettingUI.Generator`），在编译期生成无反射的 `ISettingDescriptorProvider` 和 `IPropertyValueAccessor`，完整支持 Native AOT 与裁剪发布。
- 🔌 **高度可扩展** — 可注入自定义 `ISettingDescriptorProvider` 或 `IPropertyValueAccessor`，覆盖默认实现。
- 🧭 **内置导航栏** — 各设置分组以树形列表展示在侧边栏，点击即可滚动定位到对应区域。
- 🎯 **扩展控件支持** — 内置 ColorPicker、DatePicker、TimePicker、NumericUpDown 等扩展控件。
- 🔄 **MVVM 支持** — 完美支持 CommunityToolkit.Mvvm 的 `[ObservableProperty]` 特性，可在 partial 类上使用。

## 安装

根据你的 UI 框架选择对应的包：

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" />

<!-- Ursa (带 Ursa 主题的 Avalonia) -->
<PackageReference Include="AutoSettingUI.Ursa" />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF" />
```

> **注意：** `AutoSettingUI.Core` 会作为依赖自动引入。`AutoSettingUI.Generator` 仅在 AOT 场景需要，需要在应用项目中单独引用。

**目标框架：** `AutoSettingUI.Core`、`AutoSettingUI.Avalonia`、`AutoSettingUI.Ursa` 支持 `net8.0;net9.0;net10.0`，`AutoSettingUI.WPF` 支持 `net8.0-windows;net9.0-windows;net10.0-windows`。

**Avalonia 版本范围：** `AutoSettingUI.Avalonia` 为 `[11.0.0,12.0.0)`；`AutoSettingUI.Ursa` 为 `[11.1.1,12.0.0)`（Ursa 1.13.0 依赖 Avalonia >= 11.1.1）。

## 快速上手

这部分基于 demo 的最小端到端使用示例，覆盖 Avalonia、Ursa、WPF，并包含 AOT 支持。

### 1. 添加包 / 引用

```xml
<!-- Avalonia -->
<PackageReference Include="AutoSettingUI.Avalonia" />

<!-- Ursa (带 Ursa 主题的 Avalonia) -->
<PackageReference Include="AutoSettingUI.Ursa"  />

<!-- WPF -->
<PackageReference Include="AutoSettingUI.WPF"  />
```

如需 AOT 支持，请在 **应用项目** 中额外引用源代码生成器：

```xml
<ProjectReference Include="..\..\src\AutoSettingUI.Generator\AutoSettingUI.Generator.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### 2. 定义设置模型

```csharp
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Avalonia.Attributes; // 或 AutoSettingUI.Ursa.Attributes

[SettingUI]
public sealed class AppSettings
{
    [Title("Volume")]
    [ControlBinding(typeof(Avalonia.Controls.Slider), "Value", nameof(CreateVolume))]
    public double Volume { get; set; } = 50;

    public Avalonia.Controls.Slider CreateVolume()
        => new Avalonia.Controls.Slider { Minimum = 0, Maximum = 100, Value = 50 };

    [Title("Enable Feature")]
    [CheckBox]
    public bool EnableFeature { get; set; } = true;

    [Title("Font Size")]
    [NumericUpDown(Minimum = 8, Maximum = 32, Increment = 1)]
    public int FontSize { get; set; } = 14;

    [Title("Tags")]
    public System.Collections.ObjectModel.ObservableCollection<string> Tags { get; set; }
        = new() { "Important", "Work" };
}
```

#### 配合 CommunityToolkit.Mvvm 使用

AutoSettingUI 支持 CommunityToolkit.Mvvm 的 `[ObservableProperty]` 特性。使用 partial 类和私有字段：

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using AutoSettingUI.Core.Attributes;

[SettingUI]
[MainHeader("应用程序设置")]
public partial class ApplicationSettings : ObservableObject
{
    [Title("应用程序名称")]
    [ObservableProperty]
    private string _appName = "我的应用程序";

    [ObservableProperty]
    private string _version = "1.0.0";

    [Title("启用日志")]
    [ObservableProperty]
    private bool _enableLogging;
}
```

> **注意：** 源码生成器会自动检测标记了 `[ObservableProperty]` 的字段，并为对应的属性生成 UI。字段命名约定（`_fieldName` 或 `m_fieldName`）会自动转换为属性名（`FieldName`）。

### 3. 绑定面板

Avalonia:

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.Avalonia.Controls;assembly=AutoSettingUI.Avalonia">
    <auto:AvaloniaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

Ursa:

```xml
<Window xmlns:ursa="clr-namespace:AutoSettingUI.Ursa.Controls;assembly=AutoSettingUI.Ursa">
    <ursa:UrsaAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

WPF:

```xml
<Window xmlns:auto="clr-namespace:AutoSettingUI.WPF.Controls;assembly=AutoSettingUI.WPF">
    <auto:WpfAutoSettingPanel Targets="{Binding Targets}" />
</Window>
```

ViewModel:

```csharp
public class MainViewModel
{
    public IEnumerable<object> Targets { get; } = new object[]
    {
        new AppSettings()
    };
}
```

### 4. AOT 支持（可选）

如需 Native AOT 或裁剪发布，请确保生成器在 **应用项目** 中被引用。多数情况下会自动使用生成的 Provider。如果 AOT 下出现回退为 TextBox，可以手动指定：

```csharp
using AutoSettingUI.Core.Registry;
using AutoSettingUI.Generated;

var provider = new GeneratedSettingProvider();
AotSettingRegistry.Provider = provider;
AotSettingRegistry.Accessor = provider;
```

### 5. 发布 AOT（示例）

```bash
# Ursa demo
pwsh ./publish-demo.ps1 -Framework Avalonia -Mode AOT
```

### 说明

- 需要查看生成代码时，可启用 `EmitCompilerGeneratedFiles=true`。
- `CollectionEditor(AllowEditItems = false)` 可将集合行设为只读。

## 扩展控件

AutoSettingUI 提供了多种扩展控件特性，用于特殊输入场景：

### ColorPicker（Avalonia/Ursa）

> **重要：** ColorPicker 需要在 `App.axaml` 中添加额外的样式引用：

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>
</Application.Styles>
```

使用示例：

```csharp
using Avalonia.Media;
using AutoSettingUI.Avalonia.Attributes; // 或 AutoSettingUI.Ursa.Attributes

[SettingUI]
public class ThemeSettings
{
    [Title("主题颜色")]
    [ColorPicker]
    public Color AccentColor { get; set; } = Colors.DodgerBlue;
}
```

### 其他扩展控件

| 特性               | 框架      | 说明                     |
| ------------------ | --------- | ------------------------ |
| `[CheckBox]`       | 所有      | 布尔属性显示为复选框     |
| `[DatePicker]`     | 所有      | DateTime 日期选择        |
| `[TimePicker]`     | 所有      | TimeSpan 时间选择        |
| `[NumericUpDown]`  | Avalonia  | 数值增减输入             |
| `[ColorPicker]`    | Avalonia  | 颜色选择                 |
| `[TagInput]`       | Ursa      | 字符串集合标签输入       |
| `[IPv4Box]`        | Ursa      | IP 地址输入              |

## 可用特性一览

| 特性               | 目标      | 说明                       |
| ------------------ | --------- | -------------------------- |
| `[SettingUI]`      | 类        | 标记类以生成 UI            |
| `[MainHeader]`     | 类        | 设置分区标题               |
| `[Title]`          | 属性      | 设置属性标签               |
| `[SubHeader]`      | 属性      | 创建子分区                 |
| `[Hide]`           | 属性      | 从 UI 中隐藏               |
| `[Range]`          | 属性      | 数值范围（渲染为滑块）     |
| `[ItemsSource]`    | 属性      | 下拉框数据源               |
| `[ControlBinding]` | 属性      | 自定义控件绑定             |
| `[ReadOnly]`       | 属性      | 设置为只读                 |
| `[Password]`       | 属性      | 密码输入框                 |
| `[Placeholder]`    | 属性      | 输入框占位文本             |
| `[Layout]`         | 属性      | 自定义布局（宽度、高度）   |
| `[Validation]`     | 属性      | 自定义验证方法             |
| `[DisplayOrder]`   | 属性      | 控制显示顺序（数值小在前）|

## 自定义控件绑定

通过继承 `ControlBindingAttribute` 创建自定义控件特性：

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") 
    {
    }
}
```

复杂控件可使用工厂方法：

```csharp
public sealed class NumericUpDownAttribute : ControlBindingAttribute
{
    public double Minimum { get; set; } = 0;
    public double Maximum { get; set; } = 100;
    
    public NumericUpDownAttribute() 
        : base(typeof(NumericUpDown), "Value", nameof(CreateControl)) 
    {
    }
    
    public Control CreateControl(Type propertyType)
    {
        return new NumericUpDown
        {
            Minimum = (decimal)Minimum,
            Maximum = (decimal)Maximum
        };
    }
}
```

## 包说明

| 包名                         | 用途                              |
| ---------------------------- | --------------------------------- |
| `AutoSettingUI.Core`         | 特性、接口与模型。                |
| `AutoSettingUI.Generator`    | Roslyn 增量源生成器（Analyzer）。 |
| `AutoSettingUI.Extension.Shared` | 共享验证和控件帮助类。        |
| `AutoSettingUI.Avalonia`     | Avalonia UI 面板扩展。            |
| `AutoSettingUI.Ursa`         | Ursa 主题 Avalonia 面板扩展。     |
| `AutoSettingUI.WPF`          | WPF 面板扩展。                    |

## 文档

- [架构说明](manual/architecture.md) — 项目结构与数据流
- [特性参考](manual/attributes.md) — 所有可用特性
- [框架扩展](manual/extensions.md) — 各框架特定用法
- [AOT 源生成器](manual/aot-source-generator.md) — AOT 支持详情


## 许可证

MIT
