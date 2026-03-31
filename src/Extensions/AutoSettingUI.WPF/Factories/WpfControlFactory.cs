using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;

namespace AutoSettingUI.WPF.Factories;

/// <summary>
/// Factory for creating WPF controls for setting properties.
/// </summary>
public class WpfControlFactory
{
    private readonly SettingClassDescriptor? _classDescriptor;
    private readonly IPropertyValueAccessor? _accessor;

    /// <summary>
    /// Event raised when a control with dynamic read-only or CanExecute state is created.
    /// </summary>
    public event Action<FrameworkElement, Func<bool>>? ReadOnlyControlCreated;

    /// <summary>
    /// Event raised when a control with dynamic visibility state is created.
    /// </summary>
    public event Action<FrameworkElement, Func<bool>>? VisibilityControlCreated;

    /// <summary>
    /// Event raised when a control that needs value updates on property change is created.
    /// </summary>
    public event Action<FrameworkElement, string, object, Action<FrameworkElement, object?>>? ValueControlCreated;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfControlFactory"/> class with class descriptor and property accessor.
    /// </summary>
    /// <param name="classDescriptor">The class descriptor containing default factory settings.</param>
    /// <param name="accessor">The property accessor for getting/setting values (AOT-compatible).</param>
    public WpfControlFactory(SettingClassDescriptor? classDescriptor, IPropertyValueAccessor? accessor)
    {
        _classDescriptor = classDescriptor;
        _accessor = accessor;
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

        // Handle delegate types (Action, Func, etc.) - create a button
        if (prop.IsDelegate)
        {
            return CreateDelegateControl(prop, target);
        }

        // Handle collection types
        if (prop.IsCollection)
        {
            return CreateCollectionControl(prop, target);
        }

        // Create control based on type
        if (prop.IsEnum)
        {
            var control = CreateEnumControl(prop, target);
            if (control is FrameworkElement fe)
            {
                ApplyAttributes(fe, prop, target);
                CheckAndRaiseVisibilityEvent(fe, prop, target);
            }
            return control;
        }

        var typeName = prop.PropertyTypeName.ToLowerInvariant();

        if (typeName == "system.boolean" || typeName == "bool")
        {
            // Use ToggleButton as default for bool in WPF as requested
            var control = CreateToggleButtonControl(prop, target);
            if (control is FrameworkElement fe)
            {
                ApplyAttributes(fe, prop, target);
                CheckAndRaiseVisibilityEvent(fe, prop, target);
            }
            return control;
        }

        if (typeName == "system.int32" || typeName == "int" ||
            typeName == "system.single" || typeName == "float" ||
            typeName == "system.double" || typeName == "double")
        {
            var control = CreateNumericControl(prop, target);
            if (control is FrameworkElement fe)
            {
                ApplyAttributes(fe, prop, target);
                CheckAndRaiseVisibilityEvent(fe, prop, target);
            }
            return control;
        }

        if (typeName == "system.string" || typeName == "string")
        {
            var control = CreateStringControl(prop, target);
            if (control is FrameworkElement fe)
            {
                ApplyAttributes(fe, prop, target);
                CheckAndRaiseVisibilityEvent(fe, prop, target);
            }
            return control;
        }

        if (typeName == "system.datetime" || typeName.Contains("datetime"))
        {
            var control = CreateDateTimeControl(prop, target);
            if (control is FrameworkElement fe)
            {
                ApplyAttributes(fe, prop, target);
                CheckAndRaiseVisibilityEvent(fe, prop, target);
            }
            return control;
        }

        // Default to text box
        var textBoxControl = CreateTextBox(prop, target);
        if (textBoxControl is FrameworkElement textBoxFe)
        {
            ApplyAttributes(textBoxFe, prop, target);
            CheckAndRaiseVisibilityEvent(textBoxFe, prop, target);
        }
        return textBoxControl;
    }

    /// <summary>
    /// Checks if the property has dynamic visibility and raises the VisibilityControlCreated event.
    /// </summary>
    private void CheckAndRaiseVisibilityEvent(FrameworkElement control, PropertyDescriptor prop, object target)
    {
        if (prop.IsHideDynamic && !string.IsNullOrEmpty(prop.HideMethodName))
        {
            control.Visibility = !IsEffectivelyHidden(prop, target) ? Visibility.Visible : Visibility.Collapsed;
            VisibilityControlCreated?.Invoke(control, () => !IsEffectivelyHidden(prop, target));
        }
    }

    private UIElement CreateBooleanControl(PropertyDescriptor prop, object target)
    {
        return CreateCheckBoxControl(prop, target);
    }

    private UIElement CreateToggleButtonControl(PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var toggleButton = new System.Windows.Controls.Primitives.ToggleButton
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = prop.DisplayName,
            IsChecked = (bool?)GetValue(target, prop.PropertyName) ?? false,
            IsEnabled = !isReadOnly,
            Padding = new Thickness(10, 5, 10, 5)
        };

        toggleButton.Checked += (s, e) => SetValue(target, prop.PropertyName, true);
        toggleButton.Unchecked += (s, e) => SetValue(target, prop.PropertyName, false);

        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            ReadOnlyControlCreated?.Invoke(toggleButton, () => IsEffectivelyReadOnly(prop, target));
        }

        return toggleButton;
    }

    private UIElement CreateCheckBoxControl(PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var checkBox = new CheckBox 
        { 
            VerticalAlignment = VerticalAlignment.Center,
            Content = prop.DisplayName,
            IsChecked = (bool?)GetValue(target, prop.PropertyName) ?? false,
            IsEnabled = !isReadOnly
        };

        checkBox.Checked += (s, e) => SetValue(target, prop.PropertyName, true);
        checkBox.Unchecked += (s, e) => SetValue(target, prop.PropertyName, false);

        // Track dynamic ReadOnly controls for refresh
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            ReadOnlyControlCreated?.Invoke(checkBox, () => IsEffectivelyReadOnly(prop, target));
        }

        return checkBox;
    }

    private UIElement CreateNumericControl(PropertyDescriptor prop, object target)
    {
        var value = GetValue(target, prop.PropertyName);
        var isReadOnly = IsEffectivelyReadOnly(prop, target);

        if (prop.HasRange)
        {
            // Use slider for ranged numeric values
            var slider = new Slider
            {
                Minimum = prop.MinValue,
                Maximum = prop.MaxValue,
                Value = Convert.ToDouble(value ?? 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = !isReadOnly
            };

            slider.ValueChanged += (s, e) =>
            {
                var newValue = ConvertValue(e.NewValue, prop.PropertyTypeName);
                SetValue(target, prop.PropertyName, newValue);
            };

            // Track dynamic ReadOnly controls for refresh
            if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
            {
                ReadOnlyControlCreated?.Invoke(slider, () => IsEffectivelyReadOnly(prop, target));
            }

            return slider;
        }
        else
        {
            // Use numeric text box
            var textBox = new TextBox
            {
                Text = value?.ToString() ?? "0",
                VerticalAlignment = VerticalAlignment.Center,
                IsReadOnly = isReadOnly
            };

            var (propertyInfo, _) = GetValidationInfo(target, prop.PropertyName);

            textBox.LostFocus += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out var num))
                {
                    var newValue = ConvertValue(num, prop.PropertyTypeName);
                    
                    if (propertyInfo != null && ValidateAndUpdateVisual(textBox, newValue, propertyInfo, target))
                    {
                        SetValue(target, prop.PropertyName, newValue);
                    }
                }
            };

            // Track dynamic ReadOnly controls for refresh
            if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
            {
                ReadOnlyControlCreated?.Invoke(textBox, () => IsEffectivelyReadOnly(prop, target));
            }

            return textBox;
        }
    }

    private UIElement CreateStringControl(PropertyDescriptor prop, object target)
    {
        var value = GetValue(target, prop.PropertyName)?.ToString() ?? "";
        var isReadOnly = IsEffectivelyReadOnly(prop, target);

        // Check for PasswordAttribute first, then fall back to name-based detection
        var propertyInfo = GetPropertyInfo(target, prop.PropertyName);
        var passwordAttr = propertyInfo?.GetCustomAttribute<PasswordAttribute>();
        
        if (passwordAttr != null || 
            prop.PropertyName.ToLowerInvariant().Contains("password") ||
            prop.PropertyName.ToLowerInvariant().Contains("pwd"))
        {
            var passwordBox = new PasswordBox
            {
                Password = value,
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = !isReadOnly
            };

            // Apply password-specific settings from attribute
            if (passwordAttr != null)
            {
                passwordBox.PasswordChar = passwordAttr.MaskChar;
                // Note: WPF PasswordBox doesn't support unmasked display
                // If Mask=false, we would need to use a TextBox instead
            }

            passwordBox.PasswordChanged += (s, e) =>
            {
                SetValue(target, prop.PropertyName, passwordBox.Password);
            };

            // Track dynamic ReadOnly controls for refresh
            if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
            {
                ReadOnlyControlCreated?.Invoke(passwordBox, () => IsEffectivelyReadOnly(prop, target));
            }

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
                VerticalContentAlignment = VerticalAlignment.Top,
                IsReadOnly = isReadOnly
            };

            textBox.TextChanged += (s, e) =>
            {
                SetValue(target, prop.PropertyName, textBox.Text);
            };

            // Track dynamic ReadOnly controls for refresh
            if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
            {
                ReadOnlyControlCreated?.Invoke(textBox, () => IsEffectivelyReadOnly(prop, target));
            }

            return textBox;
        }

        return CreateTextBox(prop, target);
    }

    private UIElement CreateTextBox(PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var textBox = new TextBox
        {
            Text = GetValue(target, prop.PropertyName)?.ToString() ?? "",
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = isReadOnly
        };

        var (propertyInfo, _) = GetValidationInfo(target, prop.PropertyName);

        // Register for value updates when property changes
        ValueControlCreated?.Invoke(textBox, prop.PropertyName, target, (ctrl, val) =>
        {
            if (ctrl is TextBox tb)
                tb.Text = val?.ToString() ?? "";
        });

        textBox.TextChanged += (s, e) =>
        {
            var newValue = textBox.Text;
            
            if (propertyInfo != null && ValidateAndUpdateVisual(textBox, newValue, propertyInfo, target))
            {
                SetValue(target, prop.PropertyName, newValue);
            }
        };

        // Track dynamic ReadOnly controls for refresh
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            ReadOnlyControlCreated?.Invoke(textBox, () => IsEffectivelyReadOnly(prop, target));
        }

        return textBox;
    }

    /// <summary>
    /// Gets the PropertyInfo for a property from the target object.
    /// </summary>
    private PropertyInfo? GetPropertyInfo(object target, string propertyName)
    {
        return target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    }

    /// <summary>
    /// Applies layout, placeholder, and description attributes to a framework element.
    /// </summary>
    private void ApplyAttributes(FrameworkElement element, PropertyDescriptor prop, object target)
    {
        var propertyInfo = GetPropertyInfo(target, prop.PropertyName);
        if (propertyInfo == null) return;

        // Apply layout attributes
        element.ApplyLayout(propertyInfo);

        // Apply placeholder to controls that support it
        if (element is Control control)
        {
            control.ApplyPlaceholder(propertyInfo);
        }

        // Apply description (tooltip)
        element.ApplyDescription(propertyInfo);
    }

    /// <summary>
    /// Validates a value and updates the control's visual state.
    /// Returns true if valid or no validations exist.
    /// </summary>
    private bool ValidateAndUpdateVisual(Control control, object? value, PropertyInfo propertyInfo, object target)
    {
        var validations = ControlLayoutHelper.GetValidations(propertyInfo);
        if (validations.Length == 0) return true;

        var isValid = ControlLayoutHelper.ValidateValue(propertyInfo, value, target, out var errorMessage);
        if (!isValid)
        {
            control.BorderBrush = System.Windows.Media.Brushes.Red;
            control.ToolTip = errorMessage;
        }
        else
        {
            control.ClearValue(Control.BorderBrushProperty);
            control.ToolTip = null;
        }
        return isValid;
    }

    /// <summary>
    /// Gets validation info for a property, caching the results.
    /// </summary>
    private (PropertyInfo? PropertyInfo, ValidationAttribute[] Validations) GetValidationInfo(object target, string propertyName)
    {
        var propertyInfo = GetPropertyInfo(target, propertyName);
        var validations = propertyInfo != null ? ControlLayoutHelper.GetValidations(propertyInfo) : Array.Empty<ValidationAttribute>();
        return (propertyInfo, validations);
    }

    private UIElement CreateEnumControl(PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var comboBox = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = !isReadOnly
        };

        // Use pre-computed enum values if available (AOT-compatible path)
        if (prop.EnumValues != null)
        {
            foreach (var name in prop.EnumValues)
            {
                comboBox.Items.Add(name);
            }

            var currentValue = GetValue(target, prop.PropertyName);
            if (currentValue is not null)
            {
                comboBox.SelectedItem = currentValue.ToString();
            }

            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is string selectedName)
                {
                    // Parse the enum value from string
                    var enumType = GetTypeFromName(prop.PropertyTypeName);
                    if (enumType is not null)
                    {
                        var newValue = Enum.Parse(enumType, selectedName);
                        SetValue(target, prop.PropertyName, newValue);
                    }
                }
            };
        }
        else
        {
            // Fallback to runtime type resolution
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
        }

        // Track dynamic ReadOnly controls for refresh
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            ReadOnlyControlCreated?.Invoke(comboBox, () => IsEffectivelyReadOnly(prop, target));
        }

        return comboBox;
    }

    private UIElement CreateDateTimeControl(PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var datePicker = new DatePicker
        {
            VerticalAlignment = VerticalAlignment.Center,
            SelectedDate = (DateTime?)GetValue(target, prop.PropertyName),
            IsEnabled = !isReadOnly
        };

        datePicker.SelectedDateChanged += (s, e) =>
        {
            if (datePicker.SelectedDate.HasValue)
            {
                SetValue(target, prop.PropertyName, datePicker.SelectedDate.Value);
            }
        };

        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            ReadOnlyControlCreated?.Invoke(datePicker, () => IsEffectivelyReadOnly(prop, target));
        }

        return datePicker;
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
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                    control = method.Invoke(null, null) as UIElement;
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Type))
                    control = method.Invoke(null, new object[] { prop.PropertyType }) as UIElement;
            }
            else
            {
                // Then, try to find the factory method on the target/settings class (static or instance)
                var targetType = target.GetType();
                method = targetType.GetMethod(factoryMethodName, BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                        control = method.Invoke(null, null) as UIElement;
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Type))
                        control = method.Invoke(null, new object[] { prop.PropertyType }) as UIElement;
                }
                else
                {
                    // Try instance method
                    method = targetType.GetMethod(factoryMethodName, BindingFlags.Instance | BindingFlags.Public);
                    if (method != null)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            control = method.Invoke(target, null) as UIElement;
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Type))
                            control = method.Invoke(target, new object[] { prop.PropertyType }) as UIElement;
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
            // Use property-specific binding property if available
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
            else if (string.IsNullOrEmpty(factoryMethodName))
            {
                // Auto-detect common dependency properties only when no factory method is used
                // When factory method is used, the factory is responsible for setting up bindings
                TryAutoBind(control, prop, target, controlType);
            }
            
            // Handle dynamic ReadOnly for custom controls
            if (control is FrameworkElement fe && prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
            {
                var isReadOnly = IsEffectivelyReadOnly(prop, target);
                if (control is TextBox textBox)
                {
                    textBox.IsReadOnly = isReadOnly;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.IsEnabled = !isReadOnly;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.IsEnabled = !isReadOnly;
                }
                else
                {
                    fe.IsEnabled = !isReadOnly;
                }
                ReadOnlyControlCreated?.Invoke(fe, () => IsEffectivelyReadOnly(prop, target));
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
        // Use the injected accessor if available (AOT-compatible path)
        if (_accessor != null)
        {
            return _accessor.GetValue(target, propertyName);
        }
        
        // Fallback to reflection
        var prop = target.GetType().GetProperty(propertyName);
        return prop?.GetValue(target);
    }

    private void SetValue(object target, string propertyName, object? value)
    {
        // Use the injected accessor if available (AOT-compatible path)
        if (_accessor != null)
        {
            _accessor.SetValue(target, propertyName, value);
            // Trigger command re-evaluation since property changes may affect CanExecute
            CommandManager.InvalidateRequerySuggested();
            return;
        }
        
        // Fallback to reflection
        var prop = target.GetType().GetProperty(propertyName);
        if (prop is not null && prop.CanWrite)
        {
            prop.SetValue(target, value);
            // Trigger command re-evaluation since property changes may affect CanExecute
            CommandManager.InvalidateRequerySuggested();
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

    #region Delegate (Command) Controls

    /// <summary>
    /// Creates a button control for delegate properties (Action, Func, etc.).
    /// </summary>
    private UIElement CreateDelegateControl(PropertyDescriptor prop, object target)
    {
        var button = new Button
        {
            Content = prop.DisplayName,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(15, 5, 15, 5),
            MinWidth = 80
        };

        // Get the delegate value
        var delegateValue = GetValue(target, prop.PropertyName) as Delegate;

        // Use Click event instead of Command for simpler IsEnabled binding
        button.Click += (s, e) => delegateValue?.DynamicInvoke();

        // Determine if button has dynamic CanExecute condition
        var hasCanExecuteCondition = !string.IsNullOrEmpty(prop.CanExecuteMethodName) ||
                                      prop.IsReadOnlyDynamic ||
                                      prop.IsReadOnly;

        if (hasCanExecuteCondition)
        {
            // Set initial enabled state
            button.IsEnabled = CanExecuteDelegate(prop, target);

            // Track button for dynamic IsEnabled updates (similar to ReadOnly controls)
            ReadOnlyControlCreated?.Invoke(button, () => !CanExecuteDelegate(prop, target));
        }

        return button;
    }

    /// <summary>
    /// Determines if a delegate command can execute.
    /// </summary>
    private bool CanExecuteDelegate(PropertyDescriptor prop, object target)
    {
        // If CanExecuteMethodName is specified, call that method
        if (!string.IsNullOrEmpty(prop.CanExecuteMethodName))
        {
            var method = target.GetType().GetMethod(prop.CanExecuteMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }
        }

        // If ReadOnly is specified, use that
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            return !IsEffectivelyReadOnly(prop, target);
        }

        // Default: always can execute
        return !prop.IsReadOnly;
    }

    #endregion

    #region Collection Controls

    /// <summary>
    /// Creates a control for editing collection properties.
    /// </summary>
    private UIElement CreateCollectionControl(PropertyDescriptor prop, object target)
    {
        // Check if a custom editor is specified
        if (!string.IsNullOrEmpty(prop.CollectionEditorTypeName))
        {
            return CreateCustomCollectionEditor(prop, target);
        }

        // Default collection editor
        return CreateDefaultCollectionEditor(prop, target);
    }

    /// <summary>
    /// Creates a custom collection editor.
    /// </summary>
    private UIElement CreateCustomCollectionEditor(PropertyDescriptor prop, object target)
    {
        var editorType = GetTypeFromName(prop.CollectionEditorTypeName!);
        if (editorType == null)
            return CreateDefaultCollectionEditor(prop, target);

        UIElement? editor = null;

        // Try factory method first
        if (!string.IsNullOrEmpty(prop.CollectionEditorFactoryMethod))
        {
            var method = editorType.GetMethod(prop.CollectionEditorFactoryMethod, BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                editor = method.Invoke(null, null) as UIElement;
            }
        }

        // Try to create instance
        if (editor == null)
        {
            try
            {
                editor = Activator.CreateInstance(editorType) as UIElement;
            }
            catch { }
        }

        if (editor != null)
        {
            // Set the collection as DataContext for binding (if editor is a FrameworkElement)
            if (editor is FrameworkElement fe)
            {
                fe.DataContext = GetValue(target, prop.PropertyName);
            }
            return editor;
        }

        return CreateDefaultCollectionEditor(prop, target);
    }

    /// <summary>
    /// Creates a default collection editor with add/remove/reorder capabilities.
    /// </summary>
    private UIElement CreateDefaultCollectionEditor(PropertyDescriptor prop, object target)
    {
        var collection = GetValue(target, prop.PropertyName) as System.Collections.IList;
        if (collection == null)
        {
            // Try to create a new list if the property is null
            var propInfo = target.GetType().GetProperty(prop.PropertyName);
            if (propInfo != null && propInfo.CanWrite)
            {
                var listType = typeof(List<>).MakeGenericType(GetTypeFromName(prop.CollectionElementTypeName ?? "System.Object") ?? typeof(object));
                collection = (System.Collections.IList?)Activator.CreateInstance(listType);
                propInfo.SetValue(target, collection);
            }
        }

        var panel = new StackPanel();

        var elementType = GetTypeFromName(prop.CollectionElementTypeName ?? "System.Object");

        // Items list with custom ItemTemplate
        var listBox = new ListBox
        {
            MinHeight = 60,
            MaxHeight = 150,
            Margin = new Thickness(0, 0, 0, 5),
            ItemsSource = collection as System.Collections.IEnumerable,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            SelectionMode = SelectionMode.Single,
            IsSynchronizedWithCurrentItem = true
        };

        // Set ItemContainerStyle to make items stretch horizontally
        var itemContainerStyle = new Style(typeof(ListBoxItem));
        itemContainerStyle.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
        itemContainerStyle.Setters.Add(new Setter(ListBoxItem.VerticalContentAlignmentProperty, VerticalAlignment.Stretch));
        itemContainerStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0)));
        listBox.ItemContainerStyle = itemContainerStyle;

        // Bind SelectedItem if SelectedItemProperty is specified
        if (!string.IsNullOrEmpty(prop.CollectionSelectedItemProperty))
        {
            var selectedItemProp = target.GetType().GetProperty(prop.CollectionSelectedItemProperty);
            if (selectedItemProp != null)
            {
                listBox.SelectionChanged += (s, e) =>
                {
                    if (listBox.SelectedItem != null)
                    {
                        try
                        {
                            selectedItemProp.SetValue(target, listBox.SelectedItem);
                        }
                        catch
                        {
                            // Ignore setting errors
                        }
                    }
                };
            }
        }

        // Create DataTemplate based on element type
        if (elementType != null)
        {
            listBox.ItemTemplate = CreateItemDataTemplate(elementType, collection, listBox, prop.CollectionSelectedItemProperty, target);
        }

        panel.Children.Add(listBox);

        // Button panel
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // Add button
        if (prop.CollectionAllowAdd)
        {
            var addBtn = new Button
            {
                Content = "+",
                Width = 25,
                Margin = new Thickness(0, 0, 5, 0),
                ToolTip = "Add item"
            };
            addBtn.Click += (s, e) =>
            {
                if (collection != null && !string.IsNullOrEmpty(prop.CollectionElementTypeName))
                {
                    var elementType = GetTypeFromName(prop.CollectionElementTypeName);
                    if (elementType != null)
                    {
                        try
                        {
                            object? newItem;
                            if (elementType.IsValueType)
                            {
                                newItem = Activator.CreateInstance(elementType);
                            }
                            else if (elementType == typeof(string))
                            {
                                newItem = ""; // Empty string for string collections
                            }
                            else if (elementType.GetConstructor(Type.EmptyTypes) != null)
                            {
                                newItem = Activator.CreateInstance(elementType);
                            }
                            else
                            {
                                // Try to find any public constructor and use default values
                                var ctor = elementType.GetConstructors().FirstOrDefault();
                                if (ctor != null)
                                {
                                    var parameters = ctor.GetParameters();
                                    var args = new object?[parameters.Length];
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        args[i] = parameters[i].ParameterType.IsValueType
                                            ? Activator.CreateInstance(parameters[i].ParameterType)
                                            : null;
                                    }
                                    newItem = ctor.Invoke(args);
                                }
                                else
                                {
                                    newItem = null;
                                }
                            }

                            if (newItem != null || elementType.IsValueType)
                            {
                                collection.Add(newItem);
                                // Refresh the ListBox
                                listBox.Items.Refresh();
                            }
                        }
                        catch { }
                    }
                }
            };
            buttonPanel.Children.Add(addBtn);
        }

        // Remove button
        if (prop.CollectionAllowRemove)
        {
            var removeBtn = new Button
            {
                Content = "-",
                Width = 25,
                Margin = new Thickness(0, 0, 5, 0),
                ToolTip = "Remove selected item"
            };
            removeBtn.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null && collection != null)
                {
                    collection.Remove(listBox.SelectedItem);
                    listBox.Items.Refresh();
                }
            };
            buttonPanel.Children.Add(removeBtn);
        }

        // Move up button
        if (prop.CollectionAllowReorder)
        {
            var upBtn = new Button
            {
                Content = "↑",
                Width = 25,
                Margin = new Thickness(0, 0, 5, 0),
                ToolTip = "Move up"
            };
            upBtn.Click += (s, e) =>
            {
                var index = listBox.SelectedIndex;
                if (index > 0 && collection != null)
                {
                    var item = collection[index];
                    collection.RemoveAt(index);
                    collection.Insert(index - 1, item);
                    listBox.SelectedIndex = index - 1;
                    listBox.Items.Refresh();
                }
            };
            buttonPanel.Children.Add(upBtn);

            // Move down button
            var downBtn = new Button
            {
                Content = "↓",
                Width = 25,
                Margin = new Thickness(0, 0, 5, 0),
                ToolTip = "Move down"
            };
            downBtn.Click += (s, e) =>
            {
                var index = listBox.SelectedIndex;
                if (index >= 0 && index < collection!.Count - 1)
                {
                    var item = collection[index];
                    collection.RemoveAt(index);
                    collection.Insert(index + 1, item);
                    listBox.SelectedIndex = index + 1;
                    listBox.Items.Refresh();
                }
            };
            buttonPanel.Children.Add(downBtn);
        }

        panel.Children.Add(buttonPanel);

        return panel;
    }

    #endregion

    #region ReadOnly Support

    /// <summary>
    /// Determines if a property is effectively read-only.
    /// </summary>
    private bool IsEffectivelyReadOnly(PropertyDescriptor prop, object target)
    {
        // If dynamic method is specified, call that
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            // First try to find a method
            var method = target.GetType().GetMethod(prop.ReadOnlyMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }

            // If no method found, try to find a property
            var property = target.GetType().GetProperty(prop.ReadOnlyMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (property != null)
            {
                var result = property.GetMethod != null && property.GetMethod.IsStatic
                    ? property.GetValue(null)
                    : property.GetValue(target);
                return result is bool b && b;
            }
        }

        return prop.IsReadOnly;
    }

    /// <summary>
    /// Determines if a property should be hidden based on Hide attribute.
    /// </summary>
    private bool IsEffectivelyHidden(PropertyDescriptor prop, object target)
    {
        if (prop.IsHideDynamic && !string.IsNullOrEmpty(prop.HideMethodName))
        {
            // First try to find a method
            var method = target.GetType().GetMethod(prop.HideMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }

            // If no method found, try to find a property
            var property = target.GetType().GetProperty(prop.HideMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (property != null)
            {
                var result = property.GetMethod != null && property.GetMethod.IsStatic
                    ? property.GetValue(null)
                    : property.GetValue(target);
                return result is bool b && b;
            }
        }

        return prop.IsHidden;
    }

    /// <summary>
    /// Creates a DataTemplate for collection items based on element type.
    /// </summary>
    private DataTemplate CreateItemDataTemplate(Type elementType, System.Collections.IList collection, ListBox listBox, string? selectedItemPropertyName, object target)
    {
        // Simple types: TextBox directly bound
        if (elementType == typeof(string) ||
            elementType == typeof(int) ||
            elementType == typeof(double) ||
            elementType == typeof(float) ||
            elementType.IsValueType)
        {
            return CreateSimpleTypeDataTemplate(elementType, collection, listBox, selectedItemPropertyName, target);
        }

        // Complex types: generate property editors
        return CreateComplexTypeDataTemplate(elementType, listBox, selectedItemPropertyName, target);
    }

    /// <summary>
    /// Creates a DataTemplate for simple types (string, int, etc.)
    /// </summary>
    private DataTemplate CreateSimpleTypeDataTemplate(Type elementType, System.Collections.IList collection, ListBox listBox, string? selectedItemPropertyName, object target)
    {
        var template = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(TextBox));
        factory.SetValue(TextBox.MarginProperty, new Thickness(0));
        factory.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        factory.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch);
        factory.SetValue(TextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
        factory.SetValue(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
        factory.SetBinding(TextBox.TextProperty, new Binding(".") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        
        // Add PreviewMouseDown to select the ListBoxItem when clicking on the TextBox
        factory.AddHandler(TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler((s, e) =>
        {
            if (s is TextBox tb)
            {
                var item = FindParent<ListBoxItem>(tb);
                if (item != null)
                {
                    SelectItemAndUpdateProperty(listBox, item.DataContext, selectedItemPropertyName, target);
                }
            }
        }));
        
        template.VisualTree = factory;
        return template;
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T result)
                return result;
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    /// <summary>
    /// Selects the item in the ListBox and updates the bound property if specified.
    /// </summary>
    private static void SelectItemAndUpdateProperty(ListBox listBox, object? itemContext, string? selectedItemPropertyName, object target)
    {
        if (itemContext == null) return;
        
        listBox.SelectedItem = itemContext;
        
        // Directly set the property if specified (bypasses SelectionChanged event issues)
        if (!string.IsNullOrEmpty(selectedItemPropertyName))
        {
            var selectedItemProp = target.GetType().GetProperty(selectedItemPropertyName);
            if (selectedItemProp != null)
            {
                try
                {
                    selectedItemProp.SetValue(target, itemContext);
                }
                catch
                {
                    // Ignore setting errors
                }
            }
        }
    }

    /// <summary>
    /// Creates a DataTemplate for complex types with multiple properties
    /// </summary>
    private DataTemplate CreateComplexTypeDataTemplate(Type elementType, ListBox listBox, string? selectedItemPropertyName, object target)
    {
        var template = new DataTemplate();
        var outerFactory = new FrameworkElementFactory(typeof(StackPanel));
        outerFactory.SetValue(StackPanel.MarginProperty, new Thickness(0));
        
        // Add PreviewMouseDown to select the ListBoxItem when clicking on any element
        outerFactory.AddHandler(StackPanel.PreviewMouseDownEvent, new MouseButtonEventHandler((s, e) =>
        {
            if (s is StackPanel sp)
            {
                var item = FindParent<ListBoxItem>(sp);
                if (item != null)
                {
                    SelectItemAndUpdateProperty(listBox, item.DataContext, selectedItemPropertyName, target);
                }
            }
        }));

        var properties = elementType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            // Create a Grid for each property row
            var rowFactory = new FrameworkElementFactory(typeof(Grid));
            rowFactory.SetValue(Grid.MarginProperty, new Thickness(0, 1, 0, 1));

            // Column definitions
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(80));
            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));

            // Label
            var labelFactory = new FrameworkElementFactory(typeof(TextBlock));
            labelFactory.SetValue(TextBlock.TextProperty, prop.Name + ":");
            labelFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            labelFactory.SetValue(TextBlock.MarginProperty, new Thickness(0, 0, 5, 0));
            labelFactory.SetValue(Grid.ColumnProperty, 0);
            rowFactory.AppendChild(labelFactory);

            // Editor control based on property type
            var editorFactory = CreateEditorFactory(prop.PropertyType, prop.Name);
            editorFactory.SetValue(Grid.ColumnProperty, 1);
            rowFactory.AppendChild(editorFactory);

            outerFactory.AppendChild(rowFactory);
        }

        template.VisualTree = outerFactory;
        return template;
    }

    /// <summary>
    /// Creates a FrameworkElementFactory for an editor control based on property type.
    /// </summary>
    private FrameworkElementFactory CreateEditorFactory(Type propertyType, string propertyName)
    {
        if (propertyType == typeof(string))
        {
            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            factory.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            factory.SetValue(TextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            factory.SetValue(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            factory.SetBinding(TextBox.TextProperty, new Binding(propertyName) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            return factory;
        }
        else if (propertyType == typeof(bool))
        {
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetValue(CheckBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            factory.SetBinding(CheckBox.IsCheckedProperty, new Binding(propertyName) { Mode = BindingMode.TwoWay });
            return factory;
        }
        else if (propertyType == typeof(int) || propertyType == typeof(double) || propertyType == typeof(float))
        {
            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            factory.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            factory.SetValue(TextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            factory.SetValue(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            factory.SetBinding(TextBox.TextProperty, new Binding(propertyName) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            return factory;
        }
        else if (propertyType.IsEnum)
        {
            var factory = new FrameworkElementFactory(typeof(ComboBox));
            factory.SetValue(ComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            factory.SetValue(ComboBox.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            factory.SetValue(ComboBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(ComboBox.ItemsSourceProperty, Enum.GetNames(propertyType));
            factory.SetBinding(ComboBox.SelectedItemProperty, new Binding(propertyName) { Mode = BindingMode.TwoWay, Converter = new EnumToStringConverter(propertyType) });
            return factory;
        }
        else
        {
            // Default: read-only text
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding(propertyName) { Mode = BindingMode.OneWay });
            return factory;
        }
    }

    /// <summary>
    /// Creates an editor control for a property type with a callback for value changes.
    /// </summary>
    private UIElement CreatePropertyEditorControl(Type propertyType, object? currentValue, Action<object?> valueChangedCallback)
    {
        if (propertyType == typeof(string))
        {
            var textBox = new TextBox { Text = currentValue?.ToString() ?? "" };
            textBox.TextChanged += (s, e) => valueChangedCallback(textBox.Text);
            return textBox;
        }
        else if (propertyType == typeof(bool))
        {
            var checkBox = new CheckBox { IsChecked = (bool?)currentValue ?? false };
            checkBox.Checked += (s, e) => valueChangedCallback(true);
            checkBox.Unchecked += (s, e) => valueChangedCallback(false);
            return checkBox;
        }
        else if (propertyType == typeof(int))
        {
            var textBox = new TextBox { Text = currentValue?.ToString() ?? "0" };
            textBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(textBox.Text, out var val))
                    valueChangedCallback(val);
            };
            return textBox;
        }
        else if (propertyType == typeof(double))
        {
            var textBox = new TextBox { Text = currentValue?.ToString() ?? "0" };
            textBox.TextChanged += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out var val))
                    valueChangedCallback(val);
            };
            return textBox;
        }
        else if (propertyType == typeof(float))
        {
            var textBox = new TextBox { Text = currentValue?.ToString() ?? "0" };
            textBox.TextChanged += (s, e) =>
            {
                if (float.TryParse(textBox.Text, out var val))
                    valueChangedCallback(val);
            };
            return textBox;
        }
        else if (propertyType.IsEnum)
        {
            var comboBox = new ComboBox();
            foreach (var name in Enum.GetNames(propertyType))
            {
                comboBox.Items.Add(name);
            }
            comboBox.SelectedItem = currentValue?.ToString();
            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is string selectedName)
                {
                    var enumValue = Enum.Parse(propertyType, selectedName);
                    valueChangedCallback(enumValue);
                }
            };
            return comboBox;
        }
        else
        {
            // Default: string representation
            var textBox = new TextBox { Text = currentValue?.ToString() ?? "", IsReadOnly = true };
            return textBox;
        }
    }

    #endregion
}

/// <summary>
/// A delegate command that supports explicit CanExecute notifications.
/// Uses both CommandManager.RequerySuggested for automatic WPF integration
/// and explicit event invocation for immediate updates.
/// </summary>
public class DelegateCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    private EventHandler? _canExecuteChanged;

    public event EventHandler? CanExecuteChanged
    {
        add
        {
            _canExecuteChanged += value;
            CommandManager.RequerySuggested += value;
        }
        remove
        {
            _canExecuteChanged -= value;
            CommandManager.RequerySuggested -= value;
        }
    }

    public DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    /// <summary>
    /// Raises CanExecuteChanged to immediately notify subscribers.
    /// Also calls CommandManager.InvalidateRequerySuggested for WPF integration.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        CommandManager.InvalidateRequerySuggested();
    }
}

/// <summary>
/// Converter for enum to string binding in ComboBox.
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    private readonly Type _enumType;

    public EnumToStringConverter(Type enumType)
    {
        _enumType = enumType;
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return Enum.Parse(_enumType, str);
        }
        return Enum.GetValues(_enumType).GetValue(0)!;
    }
}
