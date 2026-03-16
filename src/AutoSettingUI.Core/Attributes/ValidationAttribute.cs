namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies validation rules for a property value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class ValidationAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the validation pattern (regex).
    /// </summary>
    public string Pattern { get; set; } = "";

    /// <summary>
    /// Gets or sets the error message when validation fails.
    /// </summary>
    public string ErrorMessage { get; set; } = "";

    /// <summary>
    /// Gets or sets whether the value is required (not null or empty).
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the minimum length for string values.
    /// </summary>
    public int MinLength { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum length for string values.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// Gets or sets the minimum value for numeric comparisons.
    /// </summary>
    public double MinValue { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets the maximum value for numeric comparisons.
    /// </summary>
    public double MaxValue { get; set; } = double.NaN;

    /// <summary>
    /// Gets or sets a custom validation method name.
    /// The method should be in the same class and return bool.
    /// </summary>
    public string ValidateMethod { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationAttribute"/> class.
    /// </summary>
    public ValidationAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationAttribute"/> class with a pattern.
    /// </summary>
    public ValidationAttribute(string pattern, string errorMessage)
    {
        Pattern = pattern;
        ErrorMessage = errorMessage;
    }
}
