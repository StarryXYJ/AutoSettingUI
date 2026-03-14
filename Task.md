有以下bug：
1.WPF Demo项目，自定义视图的导航栏没有正确显示node
2.avalonia和ursa demo中的autoSettingPanel中的enum不是combobox实现输入


优化以下内容
1.优化avalonia和ursa demo项目中的样式，更可读
2.优化代码结构，使其更易于维护，删减掉不必要的部分，减少耦合性
3.是否实现以下功能：给一个特性，实现对自定义类型的表单项，特性指定控件类型和绑定到控件的哪个属性或者传一个工厂方法，没实现就去实现，实现了就在demo中添加样式