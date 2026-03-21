using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoSettingUI.Core.Models;

public sealed class NavigationNode : INotifyPropertyChanged
{
    private string _title;
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title 
    { 
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string? Icon { get; }

    public bool IsMainHeader { get; }

    public string SectionId { get; }

    public ObservableCollection<NavigationNode> Children { get; } = new();

    public bool IsExpanded { get; set; } = true;

    public bool IsSelected { get; set; }

    public SettingClassDescriptor? ClassDescriptor { get; }

    public SubSectionInfo? SubSection { get; }

    public object? TargetInstance { get; }

    public string? TitleKey { get; }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public NavigationNode(
        string title,
        string? icon,
        string sectionId,
        SettingClassDescriptor classDescriptor,
        object targetInstance,
        string? titleKey = null)
    {
        _title = title;
        Icon = icon;
        IsMainHeader = true;
        SectionId = sectionId;
        ClassDescriptor = classDescriptor;
        TargetInstance = targetInstance;
        TitleKey = titleKey;
    }

    public NavigationNode(
        string title,
        int order,
        string sectionId,
        SubSectionInfo subSection,
        NavigationNode parent,
        string? titleKey = null)
    {
        _title = title;
        IsMainHeader = false;
        SectionId = sectionId;
        SubSection = subSection;
        TitleKey = titleKey;
    }
}

/// <summary>
/// Represents a form section that can be scrolled to.
/// </summary>
public sealed class FormSection
{
    /// <summary>
    /// Gets the unique identifier matching the navigation node.
    /// </summary>
    public string SectionId { get; }

    /// <summary>
    /// Gets the header title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets whether this is a main header or subsection.
    /// </summary>
    public bool IsMainHeader { get; }

    /// <summary>
    /// Gets the target object instance.
    /// </summary>
    public object TargetInstance { get; }

    /// <summary>
    /// Gets the class descriptor.
    /// </summary>
    public SettingClassDescriptor ClassDescriptor { get; }

    /// <summary>
    /// Gets the subsection info (null for class-level sections).
    /// </summary>
    public SubSectionInfo? SubSection { get; }

    /// <summary>
    /// Gets the properties to display in this section.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FormSection"/> class.
    /// </summary>
    public FormSection(
        string sectionId,
        string title,
        bool isMainHeader,
        object targetInstance,
        SettingClassDescriptor classDescriptor,
        SubSectionInfo? subSection,
        IReadOnlyList<PropertyDescriptor> properties)
    {
        SectionId = sectionId;
        Title = title;
        IsMainHeader = isMainHeader;
        TargetInstance = targetInstance;
        ClassDescriptor = classDescriptor;
        SubSection = subSection;
        Properties = properties;
    }
}
