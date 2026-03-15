using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AutoSettingUI.Core.Attributes;

namespace AutoSettingUI.WPF.Factories;

/// <summary>
/// Helper class for applying layout and validation attributes to WPF controls.
/// </summary>
public static class ControlLayoutHelper
{
    /// <summary>
    /// Applies layout attributes from a property to a framework element.
    /// </summary>
    public static void ApplyLayout(this FrameworkElement element, PropertyInfo property)
    {
        var layoutAttr = property.GetCustomAttribute<LayoutAttribute>();
        if (layoutAttr == null) return;

        // Width
        if (!double.IsNaN(layoutAttr.Width))
            element.Width = layoutAttr.Width;

        // Height
        if (!double.IsNaN(layoutAttr.Height))
            element.Height = layoutAttr.Height;

        // MinWidth
        if (!double.IsNaN(layoutAttr.MinWidth))
            element.MinWidth = layoutAttr.MinWidth;

        // MinHeight
        if (!double.IsNaN(layoutAttr.MinHeight))
            element.MinHeight = layoutAttr.MinHeight;

        // MaxWidth
        if (!double.IsNaN(layoutAttr.MaxWidth))
            element.MaxWidth = layoutAttr.MaxWidth;

        // MaxHeight
        if (!double.IsNaN(layoutAttr.MaxHeight))
            element.MaxHeight = layoutAttr.MaxHeight;

        // HorizontalAlignment
        if (!string.IsNullOrEmpty(layoutAttr.HorizontalAlignment))
        {
            element.HorizontalAlignment = ParseHorizontalAlignment(layoutAttr.HorizontalAlignment);
        }

        // VerticalAlignment
        if (!string.IsNullOrEmpty(layoutAttr.VerticalAlignment))
        {
            element.VerticalAlignment = ParseVerticalAlignment(layoutAttr.VerticalAlignment);
        }

        // Margin
        if (!string.IsNullOrEmpty(layoutAttr.Margin))
        {
            element.Margin = ParseThickness(layoutAttr.Margin);
        }

        // Padding (only for controls that support it)
        if (!string.IsNullOrEmpty(layoutAttr.Padding))
        {
            var padding = ParseThickness(layoutAttr.Padding);
            switch (element)
            {
                case Control control:
                    control.Padding = padding;
                    break;
                case Border border:
                    border.Padding = padding;
                    break;
                case TextBlock textBlock:
                    // TextBlock doesn't have Padding, but we can use Margin as fallback
                    break;
            }
        }
    }

    /// <summary>
    /// Applies placeholder attribute to a control.
    /// </summary>
    public static void ApplyPlaceholder(this Control control, PropertyInfo property)
    {
        var placeholderAttr = property.GetCustomAttribute<PlaceholderAttribute>();
        if (placeholderAttr == null) return;

        // WPF doesn't have native placeholder support, but we can use Tag or ToolTip
        control.Tag = placeholderAttr.Text;
        
        // For TextBox, we could use a custom behavior or attached property
        // For now, set ToolTip as a simple solution
        control.ToolTip = placeholderAttr.Text;
    }

    /// <summary>
    /// Applies description attribute to a control.
    /// </summary>
    public static void ApplyDescription(this FrameworkElement element, PropertyInfo property)
    {
        var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr == null) return;

        element.ToolTip = descAttr.Text;
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

            // String length checks
            if (value is string stringValue)
            {
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
                    if (!System.Text.RegularExpressions.Regex.IsMatch(stringValue, validation.Pattern))
                    {
                        errorMessage = validation.ErrorMessage ?? $"{property.Name} format is invalid.";
                        return false;
                    }
                }
            }

            // Numeric range checks
            if (value is IComparable comparable)
            {
                if (!double.IsNaN(validation.MinValue))
                {
                    var min = Convert.ChangeType(validation.MinValue, comparable.GetType());
                    if (comparable.CompareTo(min) < 0)
                    {
                        errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at least {validation.MinValue}.";
                        return false;
                    }
                }

                if (!double.IsNaN(validation.MaxValue))
                {
                    var max = Convert.ChangeType(validation.MaxValue, comparable.GetType());
                    if (comparable.CompareTo(max) > 0)
                    {
                        errorMessage = validation.ErrorMessage ?? $"{property.Name} must be at most {validation.MaxValue}.";
                        return false;
                    }
                }
            }

            // Custom validation method
            if (!string.IsNullOrEmpty(validation.ValidateMethod))
            {
                var method = target.GetType().GetMethod(validation.ValidateMethod, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    var result = method.Invoke(target, new[] { value });
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
