using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.WPF.Factories;

/// <summary>
/// Factory for creating WPF controls for setting properties.
/// </summary>
public class WpfControlFactory
{
    private readonly SettingClassDescriptor? _classDescriptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfControlFactory"/> class.
    /// </summary>
    public WpfControlFactory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfControlFactory"/> class with class descriptor.
    /// </summary>
    /// <param name="classDescriptor">The class descriptor containing default factory settings.</param>
    public WpfControlFactory(SettingClassDescriptor? classDescriptor)
    {
        _classDescriptor = classDescriptor;
    }

    public UIElement CreateControl(PropertyDescriptor prop, object target)
    {
        // Check for custom control binding on property (highest priority)
        if (!string.IsNullOrEmpty(prop.CustomControlBinding))
        {
            return CreateCustomControl(prop, target, prop.CustomControlBinding, prop.CustomControlFactoryMethod);
        }

        // Check for class-level default factory (if no property-level binding specified)
        if (!string.IsNullOrEmpty(_classDescriptor?.DefaultControlFactoryTypeName))
        {
            return CreateCustomControl(prop, target, _classDescriptor.DefaultControlFactoryTypeName, _classDescriptor.DefaultFactoryMethod);
        }

        // Create control based on type
        if (prop.IsEnum)
        {
            return CreateEnumControl(prop, target);
        }

        var typeName = prop.PropertyTypeName.ToLowerInvariant();

        if (typeName == "system.boolean" || typeName == "bool")
        {
            return CreateBooleanControl(prop, target);
        }

        if (typeName == "system.int32" || typeName == "int" ||
            typeName == "system.single" || typeName == "float" ||
            typeName == "system.double" || typeName == "double")
        {
            return CreateNumericControl(prop, target);
        }

        if (typeName == "system.string" || typeName == "string")
        {
            return CreateStringControl(prop, target);
        }

        if (typeName == "system.datetime" || typeName.Contains("datetime"))
        {
            return CreateDateTimeControl(prop, target);
        }

        // Default to text box
        return CreateTextBox(prop, target);
    }

    private UIElement CreateBooleanControl(PropertyDescriptor prop, object target)
    {
        var checkBox = new CheckBox 
        { 
            VerticalAlignment = VerticalAlignment.Center,
            IsChecked = (bool?)GetValue(target, prop.PropertyName) ?? false
        };

        checkBox.Checked += (s, e) => SetValue(target, prop.PropertyName, true);
        checkBox.Unchecked += (s, e) => SetValue(target, prop.PropertyName, false);

        return checkBox;
    }

    private UIElement CreateNumericControl(PropertyDescriptor prop, object target)
    {
        var value = GetValue(target, prop.PropertyName);

        if (prop.HasRange)
        {
            // Use slider for ranged numeric values
            var slider = new Slider
            {
                Minimum = prop.MinValue,
                Maximum = prop.MaxValue,
                Value = Convert.ToDouble(value ?? 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            slider.ValueChanged += (s, e) =>
            {
                var newValue = ConvertValue(e.NewValue, prop.PropertyTypeName);
                SetValue(target, prop.PropertyName, newValue);
            };

            return slider;
        }
        else
        {
            // Use numeric text box
            var textBox = new TextBox
            {
                Text = value?.ToString() ?? "0",
                VerticalAlignment = VerticalAlignment.Center
            };

            textBox.LostFocus += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out var num))
                {
                    var newValue = ConvertValue(num, prop.PropertyTypeName);
                    SetValue(target, prop.PropertyName, newValue);
                }
            };

            return textBox;
        }
    }

    private UIElement CreateStringControl(PropertyDescriptor prop, object target)
    {
        var value = GetValue(target, prop.PropertyName)?.ToString() ?? "";

        // Check if it looks like a password field
        if (prop.PropertyName.ToLowerInvariant().Contains("password") ||
            prop.PropertyName.ToLowerInvariant().Contains("pwd"))
        {
            var passwordBox = new PasswordBox
            {
                Password = value,
                VerticalAlignment = VerticalAlignment.Center
            };

            passwordBox.PasswordChanged += (s, e) =>
            {
                SetValue(target, prop.PropertyName, passwordBox.Password);
            };

            return passwordBox;
        }

        // Check if it looks like a multiline field
        if (prop.PropertyName.ToLowerInvariant().Contains("description") ||
            prop.PropertyName.ToLowerInvariant().Contains("notes") ||
            prop.PropertyName.ToLowerInvariant().Contains("comment"))
        {
            var textBox = new TextBox
            {
                Text = value,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 80,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Top
            };

            textBox.TextChanged += (s, e) =>
            {
                SetValue(target, prop.PropertyName, textBox.Text);
            };

            return textBox;
        }

        return CreateTextBox(prop, target);
    }

    private UIElement CreateTextBox(PropertyDescriptor prop, object target)
    {
        var textBox = new TextBox
        {
            Text = GetValue(target, prop.PropertyName)?.ToString() ?? "",
            VerticalAlignment = VerticalAlignment.Center
        };

        textBox.TextChanged += (s, e) =>
        {
            SetValue(target, prop.PropertyName, textBox.Text);
        };

        return textBox;
    }

    private UIElement CreateEnumControl(PropertyDescriptor prop, object target)
    {
        var comboBox = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        // Get enum values - search in all loaded assemblies
        var enumType = GetTypeFromName(prop.PropertyTypeName);
        if (enumType is not null && enumType.IsEnum)
        {
            foreach (var name in Enum.GetNames(enumType))
            {
                comboBox.Items.Add(name);
            }

            var currentValue = GetValue(target, prop.PropertyName);
            if (currentValue is not null)
            {
                comboBox.SelectedItem = currentValue.ToString();
            }
        }

        comboBox.SelectionChanged += (s, e) =>
        {
            if (comboBox.SelectedItem is string selectedName && enumType is not null)
            {
                var newValue = Enum.Parse(enumType, selectedName);
                SetValue(target, prop.PropertyName, newValue);
            }
        };

        return comboBox;
    }

    private UIElement CreateDateTimeControl(PropertyDescriptor prop, object target)
    {
        // WPF doesn't have a built-in DateTimePicker, use TextBox as fallback
        var value = GetValue(target, prop.PropertyName);
        var textBox = new TextBox
        {
            Text = value?.ToString() ?? DateTime.Now.ToString(),
            VerticalAlignment = VerticalAlignment.Center
        };

        textBox.LostFocus += (s, e) =>
        {
            if (DateTime.TryParse(textBox.Text, out var date))
            {
                SetValue(target, prop.PropertyName, date);
            }
        };

        return textBox;
    }

    private UIElement CreateCustomControl(PropertyDescriptor prop, object target, string controlTypeName, string? factoryMethodName)
    {
        var controlType = GetTypeFromName(controlTypeName);
        if (controlType == null)
            return CreateTextBox(prop, target);

        UIElement? control = null;
        if (!string.IsNullOrEmpty(factoryMethodName))
        {
            // First, try to find the factory method on the control type (static)
            var method = controlType.GetMethod(factoryMethodName, BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                control = method.Invoke(null, null) as UIElement;
            }
            else
            {
                // Then, try to find the factory method on the target/settings class (static or instance)
                var targetType = target.GetType();
                method = targetType.GetMethod(factoryMethodName, BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    control = method.Invoke(null, null) as UIElement;
                }
                else
                {
                    // Try instance method
                    method = targetType.GetMethod(factoryMethodName, BindingFlags.Instance | BindingFlags.Public);
                    if (method != null)
                    {
                        control = method.Invoke(target, null) as UIElement;
                    }
                }
            }
        }

        if (control == null)
        {
            try
            {
                control = Activator.CreateInstance(controlType) as UIElement;
            }
            catch { }
        }

        if (control != null)
        {
            // Use property-specific binding property if available, otherwise try to auto-detect
            var bindingProperty = prop.CustomControlBindingProperty;
            if (!string.IsNullOrEmpty(bindingProperty))
            {
                var fieldInfo = controlType.GetField(bindingProperty + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (fieldInfo != null && fieldInfo.GetValue(null) is DependencyProperty dp)
                {
                    var binding = new Binding(prop.PropertyName) { Source = target, Mode = BindingMode.TwoWay };
                    BindingOperations.SetBinding(control, dp, binding);
                }
            }
            else
            {
                // Auto-detect common dependency properties
                TryAutoBind(control, prop, target, controlType);
            }
            return control;
        }

        return CreateTextBox(prop, target);
    }

    private void TryAutoBind(UIElement control, PropertyDescriptor prop, object target, Type controlType)
    {
        // Try to find a suitable dependency property based on property type
        string? propertyName = null;
        var propTypeName = prop.PropertyTypeName.ToLowerInvariant();

        if (propTypeName.Contains("bool"))
            propertyName = "IsChecked";
        else if (propTypeName.Contains("string"))
            propertyName = "Text";
        else if (propTypeName.Contains("int") || propTypeName.Contains("double") || propTypeName.Contains("float"))
            propertyName = "Value";

        if (!string.IsNullOrEmpty(propertyName))
        {
            var fieldInfo = controlType.GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null && fieldInfo.GetValue(null) is DependencyProperty dp)
            {
                var binding = new Binding(prop.PropertyName) { Source = target, Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(control, dp, binding);
            }
        }
    }

    private object? GetValue(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName);
        return prop?.GetValue(target);
    }

    private void SetValue(object target, string propertyName, object? value)
    {
        var prop = target.GetType().GetProperty(propertyName);
        if (prop is not null && prop.CanWrite)
        {
            prop.SetValue(target, value);
        }
    }

    private object ConvertValue(double value, string typeName)
    {
        var lowerType = typeName.ToLowerInvariant();

        if (lowerType.Contains("int32") || lowerType == "int")
            return (int)value;
        if (lowerType.Contains("single") || lowerType == "float")
            return (float)value;
        if (lowerType.Contains("double") || lowerType == "double")
            return value;

        return value;
    }

    private Type? GetTypeFromName(string typeName)
    {
        // Try direct type resolution first
        var type = Type.GetType(typeName);
        if (type is not null)
            return type;

        // Search in all loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type is not null)
                return type;
        }

        return null;
    }
}
