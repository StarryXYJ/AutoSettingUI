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

        // Use ordered list to preserve declaration order of sub-sections
        var subSectionList = new List<(string Title, List<PropertyDescriptor> Props)>();
        var directProperties = new List<PropertyDescriptor>();
        List<PropertyDescriptor>? currentSubProps = null;
        string? currentSubHeader = null;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip hidden properties
            if (prop.GetCustomAttribute<HideAttribute>() is not null)
                continue;

            // SubHeader: flush previous sub-section and start a new one.
            // Do NOT skip the property itself — it should be the first item in the new group.
            var subHeader = prop.GetCustomAttribute<SubHeaderAttribute>();
            if (subHeader is not null)
            {
                if (currentSubHeader is not null && currentSubProps is not null)
                    subSectionList.Add((currentSubHeader, currentSubProps));
                currentSubHeader = subHeader.Title;
                currentSubProps = new List<PropertyDescriptor>();
                // fall-through: the property itself is added below
            }

            var titleAttr = prop.GetCustomAttribute<TitleAttribute>();
            var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
            var itemsSourceAttr = prop.GetCustomAttribute<ItemsSourceAttribute>();
            var controlBindingAttr = prop.GetCustomAttribute<ControlBindingAttribute>();
            var commandCanExecuteAttr = prop.GetCustomAttribute<CommandCanExecuteAttribute>();
            var readOnlyAttr = prop.GetCustomAttribute<ReadOnlyAttribute>();
            var collectionEditorAttr = prop.GetCustomAttribute<CollectionEditorAttribute>();

            // Pre-compute enum values to avoid runtime Enum.GetValues in non-AOT path
            string[]? enumValues = null;
            if (prop.PropertyType.IsEnum)
                enumValues = Enum.GetNames(prop.PropertyType);

            // Check if property type is a delegate (Action, Func, custom delegate)
            var isDelegate = typeof(Delegate).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(Delegate);

            // Check if property is a collection
            var isCollection = typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string);

            // Get collection element type
            string? collectionElementTypeName = null;
            if (isCollection)
            {
                var elementType = GetCollectionElementType(prop.PropertyType);
                if (elementType != null)
                    collectionElementTypeName = elementType.FullName ?? elementType.Name;
            }

            var propDescriptor = new PropertyDescriptor(
                prop.Name,
                titleAttr?.Name ?? prop.Name,
                prop.PropertyType.FullName ?? prop.PropertyType.Name,
                prop.PropertyType,
                prop.PropertyType.IsEnum,
                isCollection,
                rangeAttr is not null,
                rangeAttr?.Minimum ?? 0,
                rangeAttr?.Maximum ?? 0,
                itemsSourceAttr?.SourceType.AssemblyQualifiedName,
                itemsSourceAttr?.SourcePropertyName,
                controlBindingAttr?.ControlType.AssemblyQualifiedName,
                controlBindingAttr?.BindingProperty,
                controlBindingAttr?.FactoryMethod,
                enumValues,
                isDelegate,
                commandCanExecuteAttr?.MethodName,
                readOnlyAttr?.IsReadOnly ?? false,
                readOnlyAttr?.MethodName,
                collectionEditorAttr?.EditorType?.AssemblyQualifiedName,
                collectionEditorAttr?.FactoryMethod,
                collectionEditorAttr?.AllowAdd ?? true,
                collectionEditorAttr?.AllowRemove ?? true,
                collectionEditorAttr?.AllowReorder ?? true,
                collectionElementTypeName
            );

            if (currentSubProps is not null)
                currentSubProps.Add(propDescriptor);
            else
                directProperties.Add(propDescriptor);
        }

        // Flush final sub-section
        if (currentSubHeader is not null && currentSubProps is not null)
            subSectionList.Add((currentSubHeader, currentSubProps));

        var subSections = subSectionList
            .Where(s => s.Props.Count > 0)
            .Select(s => new SubSectionInfo(s.Title, s.Props))
            .ToList();

        return new SettingClassDescriptor(
            type.FullName ?? type.Name,
            type.Name,
            mainHeaderAttr?.Title,
            directProperties,
            subSections,
            settingAttr.ControlFactory?.AssemblyQualifiedName,
            settingAttr.FactoryMethod
        );
    }

    /// <summary>
    /// Gets the element type of a collection type.
    /// </summary>
    private static Type? GetCollectionElementType(Type collectionType)
    {
        // Handle array types
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        // Handle generic types (List<T>, IList<T>, IEnumerable<T>, etc.)
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length > 0)
                return genericArgs[0];
        }

        // Check interfaces for generic IEnumerable<T>
        foreach (var iface in collectionType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericArgs = iface.GetGenericArguments();
                if (genericArgs.Length > 0)
                    return genericArgs[0];
            }
        }

        return null;
    }
}
