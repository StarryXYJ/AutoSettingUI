using System.Collections;
using System.Reflection;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.Core.Providers;

/// <summary>
/// A reflection-based setting descriptor provider.
/// This is useful for runtime scenarios where source generators are not available.
/// </summary>
public class ReflectionSettingDescriptorProvider : ISettingDescriptorProvider
{
    private readonly Dictionary<string, SettingClassDescriptor> _descriptors = new();

    /// <summary>
    /// Registers a type marked with [SettingUI] attribute.
    /// </summary>
    public void RegisterType(Type type)
    {
        var descriptor = CreateDescriptor(type);
        if (descriptor is not null)
        {
            _descriptors[type.FullName!] = descriptor;
        }
    }

    /// <summary>
    /// Registers multiple types.
    /// </summary>
    public void RegisterTypes(params Type[] types)
    {
        foreach (var type in types)
        {
            RegisterType(type);
        }
    }

    /// <inheritdoc />
    public SettingClassDescriptor? GetDescriptor(Type type)
    {
        return GetDescriptor(type.FullName ?? type.Name);
    }

    /// <inheritdoc />
    public SettingClassDescriptor? GetDescriptor(string typeName)
    {
        _descriptors.TryGetValue(typeName, out var descriptor);
        return descriptor;
    }

    /// <inheritdoc />
    public IReadOnlyList<SettingClassDescriptor> GetAllDescriptors()
    {
        return _descriptors.Values.ToList();
    }

    /// <inheritdoc />
    public bool HasDescriptor(Type type)
    {
        return HasDescriptor(type.FullName ?? type.Name);
    }

    /// <inheritdoc />
    public bool HasDescriptor(string typeName)
    {
        return _descriptors.ContainsKey(typeName);
    }

    private SettingClassDescriptor? CreateDescriptor(Type type)
    {
        var settingAttr = type.GetCustomAttribute<SettingUIAttribute>();
        if (settingAttr is null)
            return null;

        var mainHeaderAttr = type.GetCustomAttribute<MainHeaderAttribute>();

        var subHeaderGroups = new Dictionary<string, List<PropertyDescriptor>>();
        var directProperties = new List<PropertyDescriptor>();
        string? currentSubHeader = null;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Check for Hide attribute
            if (prop.GetCustomAttribute<HideAttribute>() is not null)
                continue;

            // Check for SubHeader attribute
            var subHeader = prop.GetCustomAttribute<SubHeaderAttribute>();
            if (subHeader is not null)
            {
                currentSubHeader = subHeader.Title;
                if (!subHeaderGroups.ContainsKey(subHeader.Title))
                {
                    subHeaderGroups[subHeader.Title] = new List<PropertyDescriptor>();
                }
                continue;
            }

            // Check for MainHeader on property (starts a new section)
            var propMainHeader = prop.GetCustomAttribute<MainHeaderAttribute>();
            if (propMainHeader is not null)
            {
                currentSubHeader = null; // Reset subsection
            }

            var titleAttr = prop.GetCustomAttribute<TitleAttribute>();
            var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
            var itemsSourceAttr = prop.GetCustomAttribute<ItemsSourceAttribute>();
            var controlBindingAttr = prop.GetCustomAttribute<ControlBindingAttribute>();

            var propDescriptor = new PropertyDescriptor(
                prop.Name,
                titleAttr?.Name ?? prop.Name,
                prop.PropertyType.FullName ?? prop.PropertyType.Name,
                prop.PropertyType.IsEnum,
                typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string),
                rangeAttr is not null,
                rangeAttr?.Minimum ?? 0,
                rangeAttr?.Maximum ?? 0,
                itemsSourceAttr?.SourceType.AssemblyQualifiedName,
                itemsSourceAttr?.SourcePropertyName,
                controlBindingAttr?.ControlType.AssemblyQualifiedName,
                controlBindingAttr?.BindingProperty,
                controlBindingAttr?.FactoryMethod
            );

            if (currentSubHeader is not null)
            {
                subHeaderGroups[currentSubHeader].Add(propDescriptor);
            }
            else
            {
                directProperties.Add(propDescriptor);
            }
        }

        // Build subsections
        var subSections = subHeaderGroups
            .Where(g => g.Value.Count > 0)
            .Select(g => new SubSectionInfo(g.Key, g.Value))
            .ToList();

        return new SettingClassDescriptor(
            type.FullName ?? type.Name,
            type.Name,
            mainHeaderAttr?.Title,
            settingAttr.Category,
            settingAttr.Order,
            directProperties,
            subSections,
            settingAttr.ControlFactory?.AssemblyQualifiedName,
            settingAttr.FactoryMethod
        );
    }
}
