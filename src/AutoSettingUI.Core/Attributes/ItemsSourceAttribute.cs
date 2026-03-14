namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies a source for items to populate a dropdown or list control.
/// Used with enum-like properties or collection properties.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class ItemsSourceAttribute : Attribute
{
    /// <summary>
    /// Gets the type that contains the source property.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the name of the static property or method that returns the items.
    /// </summary>
    public string SourcePropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsSourceAttribute"/> class.
    /// </summary>
    /// <param name="sourceType">The type containing the source property.</param>
    /// <param name="sourcePropertyName">The name of the source property or method.</param>
    public ItemsSourceAttribute(Type sourceType, string sourcePropertyName)
    {
        SourceType = sourceType;
        SourcePropertyName = sourcePropertyName;
    }
}
