# AutoSettingUI

[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Core?label=Core)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Avalonia?label=Avalonia)](https://www.nuget.org/packages/AutoSettingUI.Avalonia/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Ursa?label=Ursa)](https://www.nuget.org/packages/AutoSettingUI.Ursa/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.WPF?label=WPF)](https://www.nuget.org/packages/AutoSettingUI.WPF/)
[![NuGet](https://img.shields.io/nuget/v/AutoSettingUI.Generator?label=Generator)](https://www.nuget.org/packages/AutoSettingUI.Generator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/AutoSettingUI.Core?label=Downloads)](https://www.nuget.org/packages/AutoSettingUI.Core/)
[![License](https://img.shields.io/github/license/your-org/AutoSettingUI)](LICENSE)

**AutoSettingUI** 是一个 .NET 库，通过为普通 C# 对象标注特性（Attribute），自动生成设置界面面板。只需装饰好类，将实例传入控件，即可得到一个完整可交互的设置表单，无需手动布局任何 UI。

## 功能亮点

- 🎨 **多框架支持** — Avalonia、Ursa（Avalonia 主题）和 WPF
- ✨ **特性驱动渲染** — 使用特性灵活自定义每个属性的渲染方式
- ⚡ **AOT 兼容** — 增量 Roslyn 源生成器，完整支持 Native AOT 与裁剪发布
- 🔄 **MVVM 支持** — 完美支持 CommunityToolkit.Mvvm `[ObservableProperty]`
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

## 扩展控件

| 特性              | 框架     | 说明               |
| ----------------- | -------- | ------------------ |
| `[CheckBox]`      | 所有     | 布尔属性复选框     |
| `[DatePicker]`    | 所有     | DateTime 日期选择  |
| `[TimePicker]`    | 所有     | TimeSpan 时间选择  |
| `[NumericUpDown]` | Avalonia | 数值增减输入       |
| `[ColorPicker]`   | Avalonia | 颜色选择           |
| `[TagInput]`      | Ursa     | 字符串集合标签输入 |
| `[IPv4Box]`       | Ursa     | IP 地址输入        |

> **注意：** ColorPicker 需要在 `App.axaml` 中添加 `<StyleInclude Source="avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml"/>`。

## 自定义控件绑定

通过继承 `ControlBindingAttribute` 创建自定义控件特性：

```csharp
[AttributeUsage(AttributeTargets.Property)]
public sealed class DatePickerAttribute : ControlBindingAttribute
{
    public DatePickerAttribute() 
        : base(typeof(CalendarDatePicker), "SelectedDate") { }
}
```

复杂控件可使用工厂方法：

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
```

## 包说明

| 包名                      | 说明                     | 依赖                  |
| ------------------------- | ------------------------ | --------------------- |
| `AutoSettingUI.Avalonia`  | Avalonia UI 面板         | Core, Extension.Shared |
| `AutoSettingUI.Ursa`      | Ursa 主题 Avalonia 面板  | Core, Extension.Shared |
| `AutoSettingUI.WPF`       | WPF 面板                 | Core, Extension.Shared |
| `AutoSettingUI.Generator` | Roslyn 源生成器（AOT）   | 独立（Analyzer）      |
| `AutoSettingUI.Core`      | 特性、接口、模型         | 独立                  |

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
