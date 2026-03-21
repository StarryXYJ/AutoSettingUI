namespace AutoSettingUI.Core.Models;

/// <summary>
/// Describes a class marked with [SettingUI] attribute.
/// Rendering order follows the order of items in the Targets collection.
/// </summary>
public sealed class SettingClassDescriptor
{
    /// <summary>
    /// Gets the full type name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the display name for the class.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the main header title, if specified.
    /// </summary>
    public string? MainHeader { get; }
    
    /// <summary>
    /// Gets the resource key for the main header title, if using localization.
    /// </summary>
    public string? MainHeaderKey { get; }
    
    /// <summary>
    /// Gets whether the main header uses a resource key for localization.
    /// </summary>
    public bool UseMainHeaderKey => !string.IsNullOrEmpty(MainHeaderKey);

    /// <summary>
    /// Gets the default control factory type name for this class.
    /// </summary>
    public string? DefaultControlFactoryTypeName { get; }

    /// <summary>
    /// Gets the default factory method name for this class.
    /// </summary>
    public string? DefaultFactoryMethod { get; }

    /// <summary>
    /// Gets the list of property descriptors.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }

    /// <summary>
    /// Gets the list of subsections.
    /// </summary>
    public IReadOnlyList<SubSectionInfo> SubSections { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingClassDescriptor"/> class.
    /// </summary>
    public SettingClassDescriptor(
        string typeName,
        string displayName,
        string? mainHeader,
        IReadOnlyList<PropertyDescriptor> properties,
        IReadOnlyList<SubSectionInfo> subSections,
        string? defaultControlFactoryTypeName = null,
        string? defaultFactoryMethod = null,
        string? mainHeaderKey = null)
    {
        TypeName = typeName;
        DisplayName = displayName;
        MainHeader = mainHeader;
        MainHeaderKey = mainHeaderKey;
        Properties = properties;
        SubSections = subSections;
        DefaultControlFactoryTypeName = defaultControlFactoryTypeName;
        DefaultFactoryMethod = defaultFactoryMethod;
    }
}

/// <summary>
/// Represents a subsection within a settings class.
/// </summary>
public sealed class SubSectionInfo
{
    /// <summary>
    /// Gets the title of the subsection.
    /// </summary>
    public string Title { get; }
    
    /// <summary>
    /// Gets the resource key for the title, if using localization.
    /// </summary>
    public string? TitleKey { get; }
    
    /// <summary>
    /// Gets whether the title uses a resource key for localization.
    /// </summary>
    public bool UseTitleKey => !string.IsNullOrEmpty(TitleKey);

    /// <summary>
    /// Gets the properties in this subsection.
    /// </summary>
    public IReadOnlyList<PropertyDescriptor> Properties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubSectionInfo"/> class.
    /// </summary>
    public SubSectionInfo(string title, IReadOnlyList<PropertyDescriptor> properties, string? titleKey = null)
    {
        Title = title;
        TitleKey = titleKey;
        Properties = properties;
    }
}
