using System.Collections.ObjectModel;

namespace AutoSettingUI.Core.Models;

/// <summary>
/// Represents a node in the hierarchical navigation tree.
/// Rendering order follows the order of items in the Targets collection.
/// </summary>
public sealed class NavigationNode
{
    /// <summary>
    /// Gets the display title of this node.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the icon identifier (optional).
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// Gets whether this is a main header (class level).
    /// </summary>
    public bool IsMainHeader { get; }

    /// <summary>
    /// Gets the unique identifier for scrolling to this section.
    /// </summary>
    public string SectionId { get; }

    /// <summary>
    /// Gets the child nodes (subsections).
    /// </summary>
    public ObservableCollection<NavigationNode> Children { get; } = new();

    /// <summary>
    /// Gets or sets whether this node is expanded in the tree view.
    /// </summary>
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this node is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets the associated class descriptor (for class-level nodes).
    /// </summary>
    public SettingClassDescriptor? ClassDescriptor { get; }

    /// <summary>
    /// Gets the associated subsection info (for subsection nodes).
    /// </summary>
    public SubSectionInfo? SubSection { get; }

    /// <summary>
    /// Gets the target object instance this node represents.
    /// </summary>
    public object? TargetInstance { get; }

    /// <summary>
    /// Initializes a new class-level navigation node.
    /// </summary>
    public NavigationNode(
        string title,
        string? icon,
        string sectionId,
        SettingClassDescriptor classDescriptor,
        object targetInstance)
    {
        Title = title;
        Icon = icon;
        IsMainHeader = true;
        SectionId = sectionId;
        ClassDescriptor = classDescriptor;
        TargetInstance = targetInstance;
    }

    /// <summary>
    /// Initializes a new subsection navigation node.
    /// </summary>
    public NavigationNode(
        string title,
        int order,
        string sectionId,
        SubSectionInfo subSection,
        NavigationNode parent)
    {
        Title = title;
        IsMainHeader = false;
        SectionId = sectionId;
        SubSection = subSection;
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
