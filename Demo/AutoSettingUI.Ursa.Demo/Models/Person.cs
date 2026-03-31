using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoSettingUI.Ursa.Demo.Models;

public partial class Person : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private string _email = "";

    public override string ToString() => $"{Name} ({Age})";
}
