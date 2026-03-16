# AutoSettingUI

**AutoSettingUI** 是一个 .NET 库，通过为普通 C# 对象标注特性（Attribute），自动生成设置界面面板。只需装饰好类，将实例传入控件，即可得到一个完整可交互的设置表单，无需手动布局任何 UI。

## 功能亮点

- 🎨 **多框架支持** — 包含 Avalonia、Ursa（Avalonia 主题）和 WPF 三种实现。
- ✨ **特性驱动渲染** — 使用 `[Title]`、`[Range]`、`[Hide]`、`[SubHeader]`、`[ItemsSource]`、`[ControlBinding]` 等特性灵活自定义每个属性的渲染方式。
- ⚡ **AOT 兼容** — 内置 Roslyn 增量源生成器（`AutoSettingUI.Generator`），在编译期生成无反射的 `ISettingDescriptorProvider` 和 `IPropertyValueAccessor`，完整支持 Native AOT 与裁剪发布。
- 🔌 **高度可扩展** — 可注入自定义 `ISettingDescriptorProvider` 或 `IPropertyValueAccessor`，覆盖默认实现。
- 🧭 **内置导航栏** — 各设置分组以树形列表展示在侧边栏，点击即可滚动定位到对应区域。

## 快速上手

```csharp
[SettingUI(Category = "通用", Order = 0)]
public class AppSettings
{
    [Title("应用名称")]
    public string Name { get; set; } = "我的应用";

    [Title("启用深色模式")]
    public bool DarkMode { get; set; }

    [Title("音量"), Range(0, 100)]
    public int Volume { get; set; } = 50;
}
```

```xml
<!-- Avalonia -->
<avalonia:AvaloniaAutoSettingPanel
    Targets="{Binding Settings}"
    Title="应用设置" />
```

源生成器会在项目中自动生成 `GeneratedSettingProvider`，将其注入以启用完整 AOT：

```csharp
panel.DescriptorProvider = new AutoSettingUI.Generated.GeneratedSettingProvider();
panel.PropertyAccessor   = new AutoSettingUI.Generated.GeneratedSettingProvider();
```

## 包说明

| 包名                      | 用途                              |
| ------------------------- | --------------------------------- |
| `AutoSettingUI.Core`      | 特性、接口与模型。                |
| `AutoSettingUI.Generator` | Roslyn 增量源生成器（Analyzer）。 |
| `AutoSettingUI.Avalonia`  | Avalonia UI 面板扩展。            |
| `AutoSettingUI.Ursa`      | Ursa 主题 Avalonia 面板扩展。     |
| `AutoSettingUI.WPF`       | WPF 面板扩展。                    |

## 许可证

MIT
