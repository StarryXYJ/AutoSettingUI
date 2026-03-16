using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Extension.Shared;

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

        if (!double.IsNaN(layoutAttr.Width)) element.Width = layoutAttr.Width;
        if (!double.IsNaN(layoutAttr.Height)) element.Height = layoutAttr.Height;
        if (!double.IsNaN(layoutAttr.MinWidth)) element.MinWidth = layoutAttr.MinWidth;
        if (!double.IsNaN(layoutAttr.MinHeight)) element.MinHeight = layoutAttr.MinHeight;
        if (!double.IsNaN(layoutAttr.MaxWidth)) element.MaxWidth = layoutAttr.MaxWidth;
        if (!double.IsNaN(layoutAttr.MaxHeight)) element.MaxHeight = layoutAttr.MaxHeight;

        if (!string.IsNullOrEmpty(layoutAttr.HorizontalAlignment))
            element.HorizontalAlignment = ParseHorizontalAlignment(layoutAttr.HorizontalAlignment);

        if (!string.IsNullOrEmpty(layoutAttr.VerticalAlignment))
            element.VerticalAlignment = ParseVerticalAlignment(layoutAttr.VerticalAlignment);

        if (!string.IsNullOrEmpty(layoutAttr.Margin))
            element.Margin = ParseThickness(layoutAttr.Margin);

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
            }
        }
    }

    /// <summary>
    /// Applies placeholder attribute to a control.
    /// </summary>
    public static void ApplyPlaceholder(this Control control, PropertyInfo property)
    {
        var placeholderAttr = property.GetCustomAttribute<PlaceholderAttribute>();
        if (placeholderAttr != null)
        {
            control.Tag = placeholderAttr.Text;
            control.ToolTip = placeholderAttr.Text;
        }
    }

    /// <summary>
    /// Applies description attribute to a control.
    /// </summary>
    public static void ApplyDescription(this FrameworkElement element, PropertyInfo property)
    {
        var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr != null)
            element.ToolTip = descAttr.Text;
    }

    /// <summary>
    /// Gets validation attributes from a property.
    /// </summary>
    public static ValidationAttribute[] GetValidations(PropertyInfo property)
        => ValidationHelper.GetValidations(property);

    /// <summary>
    /// Validates a value against the validation attributes of a property.
    /// </summary>
    public static bool ValidateValue(PropertyInfo property, object? value, object target, out string? errorMessage)
        => ValidationHelper.ValidateValue(property, value, target, out errorMessage);

    private static HorizontalAlignment ParseHorizontalAlignment(string value)
        => value.ToLowerInvariant() switch
        {
            "left" => HorizontalAlignment.Left,
            "center" => HorizontalAlignment.Center,
            "right" => HorizontalAlignment.Right,
            "stretch" => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Stretch
        };

    private static VerticalAlignment ParseVerticalAlignment(string value)
        => value.ToLowerInvariant() switch
        {
            "top" => VerticalAlignment.Top,
            "center" => VerticalAlignment.Center,
            "bottom" => VerticalAlignment.Bottom,
            "stretch" => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Stretch
        };

    private static Thickness ParseThickness(string value)
    {
        var parts = value.Split(',').Select(p => p.Trim()).ToArray();

        if (parts.Length == 1 && double.TryParse(parts[0], out var uniform))
            return new Thickness(uniform);

        if (parts.Length == 4 &&
            double.TryParse(parts[0], out var left) &&
            double.TryParse(parts[1], out var top) &&
            double.TryParse(parts[2], out var right) &&
            double.TryParse(parts[3], out var bottom))
            return new Thickness(left, top, right, bottom);

        return new Thickness(0);
    }
}
