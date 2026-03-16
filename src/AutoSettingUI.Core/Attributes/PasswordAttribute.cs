namespace AutoSettingUI.Core.Attributes;

/// <summary>
/// Specifies that a string property should be treated as a password field.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class PasswordAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether to mask the password input.
    /// Default is true.
    /// </summary>
    public bool Mask { get; set; } = true;

    /// <summary>
    /// Gets or sets the masking character.
    /// Default is '•' (bullet character).
    /// </summary>
    public char MaskChar { get; set; } = '•';

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordAttribute"/> class.
    /// </summary>
    public PasswordAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordAttribute"/> class with masking option.
    /// </summary>
    public PasswordAttribute(bool mask)
    {
        Mask = mask;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordAttribute"/> class with custom mask character.
    /// </summary>
    public PasswordAttribute(char maskChar)
    {
        MaskChar = maskChar;
    }
}
