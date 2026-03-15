using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.Avalonia;

/// <summary>
/// Helper class for applying layout and validation attributes to Avalonia controls.
/// </summary>
public static class ControlLayoutHelper
{
    /// <summary>
    /// Applies layout attributes from a property to a control.
    /// </summary>
    public static void ApplyLayout(this Control control, PropertyInfo property)
    {
        var layoutAttr = property.GetCustomAttribute<LayoutAttribute>();
        if (layoutAttr == null) return;

        // Width
        if (!double.IsNaN(layoutAttr.Width))
            control.Width = layoutAttr.Width;

        // Height
        if (!double.IsNaN(layoutAttr.Height))
            control.Height = layoutAttr.Height;

        // MinWidth
        if (!double.IsNaN(layoutAttr.MinWidth))
            control.MinWidth = layoutAttr.MinWidth;

        // MinHeight
        if (!double.IsNaN(layoutAttr.MinHeight))
            control.MinHeight = layoutAttr.MinHeight;

        // MaxWidth
        if (!double.IsNaN(layoutAttr.MaxWidth))
            control.MaxWidth = layoutAttr.MaxWidth;

        // MaxHeight
        if (!double.IsNaN(layoutAttr.MaxHeight))
            control.MaxHeight = layoutAttr.MaxHeight;

        // HorizontalAlignment
        if (!string.IsNullOrEmpty(layoutAttr.HorizontalAlignment))
        {
            control.HorizontalAlignment = ParseHorizontalAlignment(layoutAttr.HorizontalAlignment);
        }

        // VerticalAlignment
        if (!string.IsNullOrEmpty(layoutAttr.VerticalAlignment))
        {
            control.VerticalAlignment = ParseVerticalAlignment(layoutAttr.VerticalAlignment);
        }

        // Margin
        if (!string.IsNullOrEmpty(layoutAttr.Margin))
        {
            control.Margin = ParseThickness(layoutAttr.Margin);
        }

        // Padding (only for controls that support it)
        if (!string.IsNullOrEmpty(layoutAttr.Padding))
        {
            var padding = ParseThickness(layoutAttr.Padding);
            // In Avalonia, padding is typically on the control itself via styled properties
            // We'll try to set it on ContentControl or Decorator
            if (control is ContentControl contentControl)
            {
                contentControl.Padding = padding;
            }
            else if (control is Decorator decorator)
            {
                decorator.Padding = padding;
            }
        }
    }

    /// <summary>
    /// Applies placeholder attribute to a TextBox.
    /// </summary>
    public static void ApplyPlaceholder(this TextBox textBox, PropertyInfo property)
    {
        var placeholderAttr = property.GetCustomAttribute<PlaceholderAttribute>();
        if (placeholderAttr == null) return;

        textBox.Watermark = placeholderAttr.Text;
    }

    /// <summary>
    /// Applies description attribute to a control as a tooltip.
    /// </summary>
    public static void ApplyDescription(this Control control, PropertyInfo property)
    {
        var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr == null) return;

        ToolTip.SetTip(control, descAttr.Text);
    }

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
    public static bool ValidateValue(PropertyInfo property, object? value, object target, out string? errorMessage)
    {
        errorMessage = null;
        var validations = GetValidations(property);

        foreach (var validation in validations)
        {
            // Required check
            if (validation.Required)
            {
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    errorMessage = validation.ErrorMessage ?? $"{property.Name} is required.";
                    return false;
                }
            }

            // Try to convert string value to the property type for numeric validation
            object? actualValue = value;
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                // String length checks
                if (validation.MinLength >= 0 && stringValue.Length < validation.MinLength)
                {
                    errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at least {validation.MinLength} characters.";
                    return false;
                }

                if (validation.MaxLength >= 0 && stringValue.Length > validation.MaxLength)
                {
                    errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at most {validation.MaxLength} characters.";
                    return false;
                }

                // Pattern check
                if (!string.IsNullOrEmpty(validation.Pattern))
                {
                    if (!Regex.IsMatch(stringValue, validation.Pattern))
                    {
                        errorMessage = validation.ErrorMessage ?? $"{property.Name} format is invalid.";
                        return false;
                    }
                }

                // Try to convert to property type for numeric validation
                var propType = property.PropertyType;
                if (propType != typeof(string))
                {
                    try
                    {
                        actualValue = Convert.ChangeType(stringValue, propType);
                    }
                    catch
                    {
                        // Conversion failed, keep as string
                    }
                }
            }

            // Numeric range checks
            if (actualValue is IComparable comparable)
            {
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
                    catch { /* Ignore conversion errors */ }
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
                    catch { /* Ignore conversion errors */ }
                }
            }

            // Custom validation method
            if (!string.IsNullOrEmpty(validation.ValidateMethod))
            {
                var method = target.GetType().GetMethod(validation.ValidateMethod, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    var result = method.Invoke(target, new[] { actualValue });
                    if (result is bool isValid && !isValid)
                    {
                        errorMessage = validation.ErrorMessage ?? $"{property.Name} is invalid.";
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Validates a value and updates the control's visual state.
    /// Returns true if valid or no validations exist.
    /// </summary>
    public static bool ValidateAndUpdateVisual(Control control, object? value, PropertyInfo propertyInfo, object target)
    {
        var validations = GetValidations(propertyInfo);
        if (validations.Length == 0) return true;

        var isValid = ValidateValue(propertyInfo, value, target, out var errorMessage);
        if (!isValid)
        {
            // Use attached property for border brush in Avalonia
            control.SetValue(global::Avalonia.Controls.Border.BorderBrushProperty, Brushes.Red);
            ToolTip.SetTip(control, errorMessage);
        }
        else
        {
            control.ClearValue(global::Avalonia.Controls.Border.BorderBrushProperty);
            ToolTip.SetTip(control, null);
        }
        return isValid;
    }

    private static HorizontalAlignment ParseHorizontalAlignment(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "left" => HorizontalAlignment.Left,
            "center" => HorizontalAlignment.Center,
            "right" => HorizontalAlignment.Right,
            "stretch" => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Stretch
        };
    }

    private static VerticalAlignment ParseVerticalAlignment(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "top" => VerticalAlignment.Top,
            "center" => VerticalAlignment.Center,
            "bottom" => VerticalAlignment.Bottom,
            "stretch" => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Stretch
        };
    }

    private static Thickness ParseThickness(string value)
    {
        var parts = value.Split(',').Select(p => p.Trim()).ToArray();
        
        if (parts.Length == 1 && double.TryParse(parts[0], out var uniform))
        {
            return new Thickness(uniform);
        }
        
        if (parts.Length == 4 &&
            double.TryParse(parts[0], out var left) &&
            double.TryParse(parts[1], out var top) &&
            double.TryParse(parts[2], out var right) &&
            double.TryParse(parts[3], out var bottom))
        {
            return new Thickness(left, top, right, bottom);
        }

        return new Thickness(0);
    }
}
