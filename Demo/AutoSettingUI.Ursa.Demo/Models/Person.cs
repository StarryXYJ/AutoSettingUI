using System.ComponentModel;

namespace AutoSettingUI.Ursa.Demo.Models;

/// <summary>
/// Sample class for demonstrating custom collection editing.
/// </summary>
public class Person : INotifyPropertyChanged
{
    private string _name = "";
    private int _age;
    private string _email = "";

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }

    public int Age
    {
        get => _age;
        set { _age = value; OnPropertyChanged(nameof(Age)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString() => $"{Name} ({Age})";
}
