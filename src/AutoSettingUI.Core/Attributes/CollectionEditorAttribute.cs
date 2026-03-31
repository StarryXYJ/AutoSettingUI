namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies how a collection property should be edited in the UI.
/// Can reference a custom editor control or use default collection editing behavior.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class CollectionEditorAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the custom editor control to use for this collection.
    /// If null, uses the default collection editor.
    /// </summary>
    public Type? EditorType { get; }

    /// <summary>
    /// Gets or sets the factory method name to create the editor control.
    /// </summary>
    public string? FactoryMethod { get; set; }

    /// <summary>
    /// Gets or sets whether users can add new items to the collection.
    /// Default is true.
    /// </summary>
    public bool AllowAdd { get; set; } = true;

    /// <summary>
    /// Gets or sets whether users can remove items from the collection.
    /// Default is true.
    /// </summary>
    public bool AllowRemove { get; set; } = true;

    /// <summary>
    /// Gets or sets whether users can reorder items in the collection.
    /// Default is true.
    /// </summary>
    public bool AllowReorder { get; set; } = true;

    /// <summary>
    /// Gets or sets whether users can edit items in the collection.
    /// Default is true.
    /// </summary>
    public bool AllowEditItems { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the property that should be bound to the selected item.
    /// This enables two-way binding between the ListBox's SelectedItem and a property on the target object.
    /// </summary>
    public string? SelectedItemProperty { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionEditorAttribute"/> class
    /// using the default collection editor.
    /// </summary>
    public CollectionEditorAttribute()
    {
        EditorType = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionEditorAttribute"/> class
    /// with a custom editor control type.
    /// </summary>
    /// <param name="editorType">The type of the custom editor control.</param>
    public CollectionEditorAttribute(Type editorType)
    {
        EditorType = editorType;
    }
}
