using System;
using System.Collections.Generic;

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
    /// Gets the resource key for the display name, if using localization.
    /// </summary>
    public string? DisplayNameKey { get; }
    
    /// <summary>
    /// Gets whether the display name uses a resource key for localization.
    /// </summary>
    public bool UseDisplayNameKey => !string.IsNullOrEmpty(DisplayNameKey);

    /// <summary>
    /// Gets the full type name of the property.
    /// </summary>
    public string PropertyTypeName { get; }

    /// <summary>
    /// Gets the actual type of the property.
    /// </summary>
    public Type PropertyType { get; }

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
    /// Gets a value indicating whether the property is hidden (static value).
    /// </summary>
    public bool IsHidden { get; }

    /// <summary>
    /// Gets the name of the method or property that returns whether the property is hidden.
    /// </summary>
    public string? HideMethodName { get; }

    /// <summary>
    /// Gets a value indicating whether the visibility is determined dynamically.
    /// </summary>
    public bool IsHideDynamic => !string.IsNullOrEmpty(HideMethodName);

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
    /// Gets whether users can edit items in the collection.
    /// </summary>
    public bool CollectionAllowEditItems { get; }

    /// <summary>
    /// Gets the element type name for collection properties.
    /// </summary>
    public string? CollectionElementTypeName { get; }

    /// <summary>
    /// Gets the name of the property that should be bound to the selected item.
    /// </summary>
    public string? CollectionSelectedItemProperty { get; }

    /// <summary>
    /// Gets the placeholder text for input controls, if specified.
    /// </summary>
    public string? PlaceholderText { get; }
    
    /// <summary>
    /// Gets the resource key for the placeholder text, if using localization.
    /// </summary>
    public string? PlaceholderKey { get; }
    
    /// <summary>
    /// Gets whether the placeholder text uses a resource key for localization.
    /// </summary>
    public bool UsePlaceholderKey => !string.IsNullOrEmpty(PlaceholderKey);

    /// <summary>
    /// Gets the description/tooltip text for the property, if specified.
    /// </summary>
    public string? DescriptionText { get; }
    
    /// <summary>
    /// Gets the resource key for the description text, if using localization.
    /// </summary>
    public string? DescriptionKey { get; }
    
    /// <summary>
    /// Gets whether the description text uses a resource key for localization.
    /// </summary>
    public bool UseDescriptionKey => !string.IsNullOrEmpty(DescriptionKey);

    /// <summary>
    /// Gets whether the property should be treated as a password field.
    /// </summary>
    public bool IsPassword { get; }

    /// <summary>
    /// Gets the password masking character, if specified.
    /// </summary>
    public char PasswordMaskChar { get; }

    /// <summary>
    /// Gets whether the property uses NumericUpDown settings.
    /// </summary>
    public bool IsNumericUpDown { get; }

    /// <summary>
    /// Gets the minimum value for NumericUpDown.
    /// </summary>
    public double NumericMinimum { get; }

    /// <summary>
    /// Gets the maximum value for NumericUpDown.
    /// </summary>
    public double NumericMaximum { get; }

    /// <summary>
    /// Gets the increment/step value for NumericUpDown.
    /// </summary>
    public double NumericIncrement { get; }

    /// <summary>
    /// Gets the display order of the property.
    /// Lower values are displayed first.
    /// Properties with the same order are displayed in their declaration order.
    /// </summary>
    public int DisplayOrder { get; }

    #region Layout Properties

    /// <summary>
    /// Gets the width for the control. double.NaN means Auto.
    /// </summary>
    public double LayoutWidth { get; }

    /// <summary>
    /// Gets the height for the control. double.NaN means Auto.
    /// </summary>
    public double LayoutHeight { get; }

    /// <summary>
    /// Gets the minimum width for the control.
    /// </summary>
    public double LayoutMinWidth { get; }

    /// <summary>
    /// Gets the minimum height for the control.
    /// </summary>
    public double LayoutMinHeight { get; }

    /// <summary>
    /// Gets the maximum width for the control.
    /// </summary>
    public double LayoutMaxWidth { get; }

    /// <summary>
    /// Gets the maximum height for the control.
    /// </summary>
    public double LayoutMaxHeight { get; }

    /// <summary>
    /// Gets the horizontal alignment for the control.
    /// Values: "Left", "Center", "Right", "Stretch"
    /// </summary>
    public string? LayoutHorizontalAlignment { get; }

    /// <summary>
    /// Gets the vertical alignment for the control.
    /// Values: "Top", "Center", "Bottom", "Stretch"
    /// </summary>
    public string? LayoutVerticalAlignment { get; }

    /// <summary>
    /// Gets the margin for the control.
    /// </summary>
    public string? LayoutMargin { get; }

    /// <summary>
    /// Gets the padding for the control.
    /// </summary>
    public string? LayoutPadding { get; }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDescriptor"/> class.
    /// </summary>
    public PropertyDescriptor(
        string propertyName,
        string displayName,
        string propertyTypeName,
        Type propertyType,
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
        bool isHidden = false,
        string? hideMethodName = null,
        string? collectionEditorTypeName = null,
        string? collectionEditorFactoryMethod = null,
        bool collectionAllowAdd = true,
        bool collectionAllowRemove = true,
        bool collectionAllowReorder = true,
        bool collectionAllowEditItems = true,
        string? collectionElementTypeName = null,
        string? collectionSelectedItemProperty = null,
        string? placeholderText = null,
        string? descriptionText = null,
        bool isPassword = false,
        char passwordMaskChar = '•',
        bool isNumericUpDown = false,
        double numericMinimum = 0,
        double numericMaximum = 0,
        double numericIncrement = 0,
        int displayOrder = 0,
        string? displayNameKey = null,
        string? placeholderKey = null,
        string? descriptionKey = null,
        double layoutWidth = double.NaN,
        double layoutHeight = double.NaN,
        double layoutMinWidth = double.NaN,
        double layoutMinHeight = double.NaN,
        double layoutMaxWidth = double.NaN,
        double layoutMaxHeight = double.NaN,
        string? layoutHorizontalAlignment = null,
        string? layoutVerticalAlignment = null,
        string? layoutMargin = null,
        string? layoutPadding = null)
    {
        PropertyName = propertyName;
        DisplayName = displayName;
        DisplayNameKey = displayNameKey;
        PropertyTypeName = propertyTypeName;
        PropertyType = propertyType;
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
        IsHidden = isHidden;
        HideMethodName = hideMethodName;
        CollectionEditorTypeName = collectionEditorTypeName;
        CollectionEditorFactoryMethod = collectionEditorFactoryMethod;
        CollectionAllowAdd = collectionAllowAdd;
        CollectionAllowRemove = collectionAllowRemove;
        CollectionAllowReorder = collectionAllowReorder;
        CollectionAllowEditItems = collectionAllowEditItems;
        CollectionElementTypeName = collectionElementTypeName;
        CollectionSelectedItemProperty = collectionSelectedItemProperty;
        PlaceholderText = placeholderText;
        PlaceholderKey = placeholderKey;
        DescriptionText = descriptionText;
        DescriptionKey = descriptionKey;
        IsPassword = isPassword;
        PasswordMaskChar = passwordMaskChar;
        IsNumericUpDown = isNumericUpDown;
        NumericMinimum = numericMinimum;
        NumericMaximum = numericMaximum;
        NumericIncrement = numericIncrement;
        DisplayOrder = displayOrder;
        LayoutWidth = layoutWidth;
        LayoutHeight = layoutHeight;
        LayoutMinWidth = layoutMinWidth;
        LayoutMinHeight = layoutMinHeight;
        LayoutMaxWidth = layoutMaxWidth;
        LayoutMaxHeight = layoutMaxHeight;
        LayoutHorizontalAlignment = layoutHorizontalAlignment;
        LayoutVerticalAlignment = layoutVerticalAlignment;
        LayoutMargin = layoutMargin;
        LayoutPadding = layoutPadding;
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
