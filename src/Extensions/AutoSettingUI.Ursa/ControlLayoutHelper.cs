using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Extension.Shared;

namespace AutoSettingUI.Ursa;

/// <summary>
/// Helper class for applying layout and validation attributes to Ursa controls.
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

        if (!double.IsNaN(layoutAttr.Width)) control.Width = layoutAttr.Width;
        if (!double.IsNaN(layoutAttr.Height)) control.Height = layoutAttr.Height;
        if (!double.IsNaN(layoutAttr.MinWidth)) control.MinWidth = layoutAttr.MinWidth;
        if (!double.IsNaN(layoutAttr.MinHeight)) control.MinHeight = layoutAttr.MinHeight;
        if (!double.IsNaN(layoutAttr.MaxWidth)) control.MaxWidth = layoutAttr.MaxWidth;
        if (!double.IsNaN(layoutAttr.MaxHeight)) control.MaxHeight = layoutAttr.MaxHeight;

        if (!string.IsNullOrEmpty(layoutAttr.HorizontalAlignment))
            control.HorizontalAlignment = ParseHorizontalAlignment(layoutAttr.HorizontalAlignment);

        if (!string.IsNullOrEmpty(layoutAttr.VerticalAlignment))
            control.VerticalAlignment = ParseVerticalAlignment(layoutAttr.VerticalAlignment);

        if (!string.IsNullOrEmpty(layoutAttr.Margin))
            control.Margin = ParseThickness(layoutAttr.Margin);

        if (!string.IsNullOrEmpty(layoutAttr.Padding))
        {
            var padding = ParseThickness(layoutAttr.Padding);
            if (control is ContentControl contentControl)
                contentControl.Padding = padding;
            else if (control is Decorator decorator)
                decorator.Padding = padding;
        }
    }

    /// <summary>
    /// Applies placeholder attribute to a TextBox.
    /// </summary>
    public static void ApplyPlaceholder(this TextBox textBox, PropertyInfo property)
    {
        var placeholderAttr = property.GetCustomAttribute<PlaceholderAttribute>();
        if (placeholderAttr != null)
            textBox.Watermark = placeholderAttr.Text;
    }

    /// <summary>
    /// Applies description attribute to a control as a tooltip.
    /// </summary>
    public static void ApplyDescription(this Control control, PropertyInfo property)
    {
        var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr != null)
            ToolTip.SetTip(control, descAttr.Text);
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

    /// <summary>
    /// Validates a value and updates the control's visual state.
    /// </summary>
    public static bool ValidateAndUpdateVisual(Control control, object? value, PropertyInfo propertyInfo, object target)
    {
        var validations = GetValidations(propertyInfo);
        if (validations.Length == 0) return true;

        var isValid = ValidateValue(propertyInfo, value, target, out var errorMessage);

        if (!isValid)
        {
            control.SetValue(Border.BorderBrushProperty, Brushes.Red);
            ToolTip.SetTip(control, errorMessage);
        }
        else
        {
            control.ClearValue(Border.BorderBrushProperty);
            ToolTip.SetTip(control, null);
        }

        return isValid;
    }

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
