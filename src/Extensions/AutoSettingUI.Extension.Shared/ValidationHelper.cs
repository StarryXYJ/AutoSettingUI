using System.Reflection;
using System.Text.RegularExpressions;
using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Extension.Shared;

/// <summary>
/// Shared validation logic for property values across all UI frameworks.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Gets validation attributes from a property.
    /// </summary>
    public static ValidationAttribute[] GetValidations(PropertyInfo property)
    {
        return property.GetCustomAttributes<ValidationAttribute>().ToArray();
    }

    /// <summary>
    /// Validates a value against the validation attributes of a property.
    /// </summary>
    /// <param name="property">The property being validated.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="target">The target object containing the property.</param>
    /// <param name="errorMessage">Output error message if validation fails.</param>
    /// <returns>True if validation passes, false otherwise.</returns>
    public static bool ValidateValue(PropertyInfo property, object? value, object target, out string? errorMessage)
    {
        errorMessage = null;
        var validations = GetValidations(property);

        foreach (var validation in validations)
        {
            if (!ValidateSingle(validation, property, value, target, out errorMessage))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates a single validation attribute.
    /// </summary>
    private static bool ValidateSingle(ValidationAttribute validation, PropertyInfo property, object? value, object target, out string? errorMessage)
    {
        errorMessage = null;

        if (validation.Required && IsNullOrEmpty(value))
        {
            errorMessage = validation.ErrorMessage ?? $"{property.Name} is required.";
            return false;
        }

        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        {
            if (!ValidateStringLength(validation, stringValue, property, out errorMessage))
                return false;

            if (!ValidatePattern(validation, stringValue, property, out errorMessage))
                return false;
        }

        var actualValue = TryConvertValue(value, property.PropertyType);
        if (!ValidateRange(validation, actualValue, property, out errorMessage))
            return false;

        if (!ValidateCustomMethod(validation, actualValue, target, property, out errorMessage))
            return false;

        return true;
    }

    private static bool IsNullOrEmpty(object? value)
    {
        return value == null || (value is string str && string.IsNullOrWhiteSpace(str));
    }

    private static bool ValidateStringLength(ValidationAttribute validation, string value, PropertyInfo property, out string? errorMessage)
    {
        errorMessage = null;

        if (validation.MinLength >= 0 && value.Length < validation.MinLength)
        {
            errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at least {validation.MinLength} characters.";
            return false;
        }

        if (validation.MaxLength >= 0 && value.Length > validation.MaxLength)
        {
            errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at most {validation.MaxLength} characters.";
            return false;
        }

        return true;
    }

    private static bool ValidatePattern(ValidationAttribute validation, string value, PropertyInfo property, out string? errorMessage)
    {
        errorMessage = null;

        if (!string.IsNullOrEmpty(validation.Pattern) && !Regex.IsMatch(value, validation.Pattern))
        {
            errorMessage = validation.ErrorMessage ?? $"{property.Name} format is invalid.";
            return false;
        }

        return true;
    }

    private static bool ValidateRange(ValidationAttribute validation, object? value, PropertyInfo property, out string? errorMessage)
    {
        errorMessage = null;

        if (value is not IComparable comparable)
            return true;

        if (!double.IsNaN(validation.MinValue))
        {
            try
            {
                var min = Convert.ChangeType(validation.MinValue, comparable.GetType());
                if (comparable.CompareTo(min) < 0)
                {
                    errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at least {validation.MinValue}.";
                    return false;
                }
            }
            catch { }
        }

        if (!double.IsNaN(validation.MaxValue))
        {
            try
            {
                var max = Convert.ChangeType(validation.MaxValue, comparable.GetType());
                if (comparable.CompareTo(max) > 0)
                {
                    errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at most {validation.MaxValue}.";
                    return false;
                }
            }
            catch { }
        }

        return true;
    }

    private static bool ValidateCustomMethod(ValidationAttribute validation, object? value, object target, PropertyInfo property, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrEmpty(validation.ValidateMethod))
            return true;

        var method = target.GetType().GetMethod(validation.ValidateMethod,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        if (method == null)
            return true;

        var result = method.IsStatic
            ? method.Invoke(null, new[] { value })
            : method.Invoke(target, new[] { value });

        if (result is bool isValid && !isValid)
        {
            errorMessage = validation.ErrorMessage ?? $"{property.Name} is invalid.";
            return false;
        }

        return true;
    }

    private static object? TryConvertValue(object? value, Type targetType)
    {
        if (value is not string stringValue || string.IsNullOrWhiteSpace(stringValue))
            return value;

        if (targetType == typeof(string))
            return value;

        try
        {
            return Convert.ChangeType(stringValue, targetType);
        }
        catch
        {
            return value;
        }
    }
}
