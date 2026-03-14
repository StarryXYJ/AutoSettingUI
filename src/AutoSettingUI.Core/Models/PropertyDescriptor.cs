namespace AutoSettingUI.Core.Models;

/// <summary>
/// Describes a property in a settings class.
/// </summary>
public sealed class PropertyDescriptor
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the display name for the property.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the full type name of the property.
    /// </summary>
    public string PropertyTypeName { get; }

    /// <summary>
    /// Gets a value indicating whether the property type is an enum.
    /// </summary>
    public bool IsEnum { get; }

    /// <summary>
    /// Gets a value indicating whether the property type is a collection.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    /// Gets a value indicating whether the property has a range constraint.
    /// </summary>
    public bool HasRange { get; }

    /// <summary>
    /// Gets the minimum value for range constraints.
    /// </summary>
    public double MinValue { get; }

    /// <summary>
    /// Gets the maximum value for range constraints.
    /// </summary>
    public double MaxValue { get; }

    /// <summary>
    /// Gets the items source type name, if specified.
    /// </summary>
    public string? ItemsSourceTypeName { get; }

    /// <summary>
    /// Gets the items source property name, if specified.
    /// </summary>
    public string? ItemsSourcePropertyName { get; }

    /// <summary>
    /// Gets the custom control binding type name, if specified.
    /// </summary>
    public string? CustomControlBinding { get; }

    /// <summary>
    /// Gets the property name to bind to on the custom control.
    /// </summary>
    public string? CustomControlBindingProperty { get; }

    /// <summary>
    /// Gets the factory method name to create the custom control.
    /// </summary>
    public string? CustomControlFactoryMethod { get; }

    /// <summary>
    /// Gets the pre-computed enum values as string array, if the property is an enum.
    /// This avoids runtime type resolution for AOT compatibility.
    /// </summary>
    public string[]? EnumValues { get; }

    /// <summary>
    /// Gets a value indicating whether the property type is a delegate (Action, Func, etc.).
    /// </summary>
    public bool IsDelegate { get; }

    /// <summary>
    /// Gets the name of the method to call for CanExecute evaluation on delegate properties.
    /// </summary>
    public string? CanExecuteMethodName { get; }

    /// <summary>
    /// Gets a value indicating whether the property is read-only (static value).
    /// </summary>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Gets the name of the method that returns whether the property is read-only.
    /// </summary>
    public string? ReadOnlyMethodName { get; }

    /// <summary>
    /// Gets a value indicating whether the read-only state is determined dynamically.
    /// </summary>
    public bool IsReadOnlyDynamic => !string.IsNullOrEmpty(ReadOnlyMethodName);

    /// <summary>
    /// Gets the collection editor type name, if specified.
    /// </summary>
    public string? CollectionEditorTypeName { get; }

    /// <summary>
    /// Gets the collection editor factory method name, if specified.
    /// </summary>
    public string? CollectionEditorFactoryMethod { get; }

    /// <summary>
    /// Gets whether users can add new items to the collection.
    /// </summary>
    public bool CollectionAllowAdd { get; }

    /// <summary>
    /// Gets whether users can remove items from the collection.
    /// </summary>
    public bool CollectionAllowRemove { get; }

    /// <summary>
    /// Gets whether users can reorder items in the collection.
    /// </summary>
    public bool CollectionAllowReorder { get; }

    /// <summary>
    /// Gets the element type name for collection properties.
    /// </summary>
    public string? CollectionElementTypeName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDescriptor"/> class.
    /// </summary>
    public PropertyDescriptor(
        string propertyName,
        string displayName,
        string propertyTypeName,
        bool isEnum,
        bool isCollection,
        bool hasRange,
        double minValue,
        double maxValue,
        string? itemsSourceTypeName,
        string? itemsSourcePropertyName,
        string? customControlBinding,
        string? customControlBindingProperty = null,
        string? customControlFactoryMethod = null,
        string[]? enumValues = null,
        bool isDelegate = false,
        string? canExecuteMethodName = null,
        bool isReadOnly = false,
        string? readOnlyMethodName = null,
        string? collectionEditorTypeName = null,
        string? collectionEditorFactoryMethod = null,
        bool collectionAllowAdd = true,
        bool collectionAllowRemove = true,
        bool collectionAllowReorder = true,
        string? collectionElementTypeName = null)
    {
        PropertyName = propertyName;
        DisplayName = displayName;
        PropertyTypeName = propertyTypeName;
        IsEnum = isEnum;
        IsCollection = isCollection;
        HasRange = hasRange;
        MinValue = minValue;
        MaxValue = maxValue;
        ItemsSourceTypeName = itemsSourceTypeName;
        ItemsSourcePropertyName = itemsSourcePropertyName;
        CustomControlBinding = customControlBinding;
        CustomControlBindingProperty = customControlBindingProperty;
        CustomControlFactoryMethod = customControlFactoryMethod;
        EnumValues = enumValues;
        IsDelegate = isDelegate;
        CanExecuteMethodName = canExecuteMethodName;
        IsReadOnly = isReadOnly;
        ReadOnlyMethodName = readOnlyMethodName;
        CollectionEditorTypeName = collectionEditorTypeName;
        CollectionEditorFactoryMethod = collectionEditorFactoryMethod;
        CollectionAllowAdd = collectionAllowAdd;
        CollectionAllowRemove = collectionAllowRemove;
        CollectionAllowReorder = collectionAllowReorder;
        CollectionElementTypeName = collectionElementTypeName;
    }
}

/// <summary>
/// Represents information about an items source.
/// </summary>
public sealed class ItemsSourceInfo
{
    /// <summary>
    /// Gets the type containing the source.
    /// </summary>
    public string SourceTypeName { get; }

    /// <summary>
    /// Gets the property or method name.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsSourceInfo"/> class.
    /// </summary>
    public ItemsSourceInfo(string sourceTypeName, string memberName)
    {
        SourceTypeName = sourceTypeName;
        MemberName = memberName;
    }
}
