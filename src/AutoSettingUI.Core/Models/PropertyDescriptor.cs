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
        string? customControlFactoryMethod = null)
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
