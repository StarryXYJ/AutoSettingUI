using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AutoSettingUI.Core.Attributes;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.Core.Providers;
using AutoSettingUI.Avalonia;

namespace AutoSettingUI.Avalonia.Controls;

/// <summary>
/// Avalonia implementation of the AutoSettingPanel control.
/// </summary>
public class AvaloniaAutoSettingPanel : TemplatedControl
{
    private ScrollViewer? _formScrollViewer;
    private global::Avalonia.Controls.TreeView? _navigationTree;
    private ISettingDescriptorProvider? _effectiveProvider;
    private IPropertyValueAccessor? _accessor;
    private readonly List<FormSection> _formSections = new();
    private readonly ObservableCollection<NavigationNode> _navigationNodes = new();
    private readonly Dictionary<string, global::Avalonia.Controls.Control> _sectionControlMap = new();
    private IEnumerable? _previousTargets;
    private readonly List<DelegateCommand> _commands = new();
    private readonly List<INotifyPropertyChanged> _subscribedTargets = new();
    private readonly List<(global::Avalonia.Controls.Control Control, Func<bool> IsReadOnlyGetter)> _readOnlyControls = new();

    #region Styled Properties

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Targets"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> TargetsProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, IEnumerable?>(nameof(Targets));

    /// <summary>
    /// Defines the <see cref="DescriptorProvider"/> property.
    /// </summary>
    public static readonly StyledProperty<ISettingDescriptorProvider?> DescriptorProviderProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, ISettingDescriptorProvider?>(nameof(DescriptorProvider));

    /// <summary>
    /// Defines the <see cref="ShowNavigation"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowNavigationProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, bool>(nameof(ShowNavigation), true);

    /// <summary>
    /// Defines the <see cref="NavigationWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> NavigationWidthProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, double>(nameof(NavigationWidth), 200);

    /// <summary>
    /// Defines the <see cref="PropertyAccessor"/> property.
    /// </summary>
    public static readonly StyledProperty<IPropertyValueAccessor?> PropertyAccessorProperty =
        AvaloniaProperty.Register<AvaloniaAutoSettingPanel, IPropertyValueAccessor?>(nameof(PropertyAccessor));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the title of the panel.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of target objects to display.
    /// </summary>
    public IEnumerable? Targets
    {
        get => GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    /// <summary>
    /// Gets or sets the current target object being edited (for backward compatibility).
    /// </summary>
    [Obsolete("Use Targets property instead")]
    public object? CurrentTarget
    {
        get => Targets?.Cast<object?>().FirstOrDefault();
        set => Targets = value is not null ? new[] { value } : null;
    }

    /// <summary>
    /// Gets or sets the descriptor provider.
    /// </summary>
    public ISettingDescriptorProvider? DescriptorProvider
    {
        get => GetValue(DescriptorProviderProperty);
        set => SetValue(DescriptorProviderProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show the navigation panel.
    /// </summary>
    public bool ShowNavigation
    {
        get => GetValue(ShowNavigationProperty);
        set => SetValue(ShowNavigationProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the navigation panel.
    /// </summary>
    public double NavigationWidth
    {
        get => GetValue(NavigationWidthProperty);
        set => SetValue(NavigationWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the property accessor used to get and set values on target objects.
    /// Inject the source-generated <c>GeneratedSettingProvider</c> here for AOT compatibility.
    /// Falls back to <see cref="ReflectionPropertyAccessor"/> if not set.
    /// </summary>
    public IPropertyValueAccessor? PropertyAccessor
    {
        get => GetValue(PropertyAccessorProperty);
        set => SetValue(PropertyAccessorProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaAutoSettingPanel"/> class.
    /// </summary>
    public AvaloniaAutoSettingPanel()
    {
    }

    /// <summary>
    /// Refreshes all dynamic states (CanExecute for commands, dynamic ReadOnly states).
    /// Call this method when underlying data changes that affects CanExecute or ReadOnly conditions.
    /// </summary>
    public void Refresh()
    {
        // Raise CanExecuteChanged for all commands
        foreach (var command in _commands)
        {
            command.RaiseCanExecuteChanged();
        }

        // Update IsEnabled/IsReadOnly state for controls with dynamic conditions
        foreach (var (control, isReadOnlyGetter) in _readOnlyControls)
        {
            var isReadOnly = isReadOnlyGetter();
            if (control is TextBox textBox)
            {
                textBox.IsReadOnly = isReadOnly;
            }
            else if (control is global::Avalonia.Controls.Primitives.ToggleButton toggleButton)
            {
                toggleButton.IsEnabled = !isReadOnly;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.IsEnabled = !isReadOnly;
            }
            else if (control is Slider slider)
            {
                slider.IsEnabled = !isReadOnly;
            }
            else if (control is Button button)
            {
                button.IsEnabled = !isReadOnly;
            }
            else
            {
                // Generic fallback
                control.IsEnabled = !isReadOnly;
            }
        }
    }

    /// <summary>
    /// Handles PropertyChanged events from target objects to refresh commands.
    /// </summary>
    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Refresh all commands when any property changes
        Refresh();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _formScrollViewer = e.NameScope.Find<ScrollViewer>("PART_FormScrollViewer");
        _navigationTree = e.NameScope.Find<global::Avalonia.Controls.TreeView>("PART_Navigation");

        if (_navigationTree is not null)
        {
            _navigationTree.SelectionChanged += OnNavigationSelectionChanged;
        }

        InitializeProvider();
        
        // Only build UI if we have all required components
        if (_effectiveProvider is not null && Targets is not null && _formScrollViewer is not null)
        {
            BuildUI();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DescriptorProviderProperty && change.NewValue is ISettingDescriptorProvider provider)
        {
            _effectiveProvider = provider;
            if (provider is IPropertyValueAccessor acc)
                _accessor = acc;
            BuildUI();
        }
        else if (change.Property == PropertyAccessorProperty && change.NewValue is IPropertyValueAccessor injectedAcc)
        {
            _accessor = injectedAcc;
            BuildUI();
        }
        else if (change.Property == TargetsProperty)
        {
            // Unsubscribe from old collection changes
            if (_previousTargets is INotifyCollectionChanged oldNotify)
            {
                oldNotify.CollectionChanged -= OnTargetsCollectionChanged;
            }
            
            // Subscribe to new collection changes
            if (change.NewValue is INotifyCollectionChanged newNotify)
            {
                newNotify.CollectionChanged += OnTargetsCollectionChanged;
            }
            _previousTargets = change.NewValue as IEnumerable;
            
            InitializeProvider();
            BuildUI();
        }
    }
    
    private void OnTargetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null && _effectiveProvider is ReflectionSettingDescriptorProvider reflProvider)
        {
            foreach (var item in e.NewItems)
            {
                if (item is not null)
                    reflProvider.RegisterType(item.GetType());
            }
        }

        if (_formScrollViewer is not null)
            BuildUI();
    }

    private void InitializeProvider()
    {
        if (_effectiveProvider is not null) return;

        // 1. Priority: Global AOT Registry (Zero-code AOT support)
        if (AutoSettingUI.Core.Registry.AotSettingRegistry.Provider != null)
        {
            _effectiveProvider = AutoSettingUI.Core.Registry.AotSettingRegistry.Provider;
            _accessor = AutoSettingUI.Core.Registry.AotSettingRegistry.Accessor;
            return;
        }

        // 2. Injected accessor that is also a provider
        if (_accessor is ISettingDescriptorProvider accessorProvider)
        {
            _effectiveProvider = accessorProvider;
            return;
        }

        if (_accessor is null)
        {
            _accessor = TryCreateGeneratedProvider();
            if (_accessor is ISettingDescriptorProvider genProvider)
            {
                _effectiveProvider = genProvider;
                return;
            }
        }

        // Fallback to reflection-based provider (non-AOT)
        var reflProvider = new ReflectionSettingDescriptorProvider();
        _effectiveProvider = reflProvider;
        _accessor ??= new ReflectionPropertyAccessor();

        if (Targets is not null)
        {
            foreach (var target in Targets)
            {
                if (target is not null)
                    reflProvider.RegisterType(target.GetType());
            }
        }
    }

    /// <summary>
    /// Tries to create the source-generated provider via reflection.
    /// This allows AOT-compatible usage without explicit reference to generated code.
    /// </summary>
    private static IPropertyValueAccessor? TryCreateGeneratedProvider()
    {
        try
        {
            // Look for the generated provider in all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var providerType = assembly.GetType("AutoSettingUI.Generated.GeneratedSettingProvider");
                if (providerType is not null)
                {
                    var instance = Activator.CreateInstance(providerType);
                    if (instance is IPropertyValueAccessor accessor)
                        return accessor;
                }
            }
        }
        catch
        {
            // Ignore errors and fall back to reflection
        }
        return null;
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_navigationTree?.SelectedItem is NavigationNode node)
        {
            ScrollToSection(node.SectionId);
        }
    }

    private void ScrollToSection(string sectionId)
    {
        if (_formScrollViewer is null) return;
        
        // O(1) lookup using the dictionary
        if (_sectionControlMap.TryGetValue(sectionId, out var control))
        {
            control.BringIntoView();
        }
    }

    private void BuildUI()
    {
        if (_effectiveProvider is null || Targets is null || _formScrollViewer is null) return;

        BuildForm();
        
        // Always reset ItemsSource to ensure TreeView refreshes properly
        if (_navigationTree is not null)
        {
            _navigationTree.ItemsSource = null;
            _navigationTree.ItemsSource = _navigationNodes;
        }
    }

    private void BuildForm()
    {
        if (_formScrollViewer is null || Targets is null) return;

        // Unsubscribe from old targets
        foreach (var target in _subscribedTargets)
        {
            target.PropertyChanged -= OnTargetPropertyChanged;
        }
        _subscribedTargets.Clear();

        // Clear previous data
        _formSections.Clear();
        _navigationNodes.Clear();
        _sectionControlMap.Clear();
        _commands.Clear();
        _readOnlyControls.Clear();

        var formPanel = new StackPanel
        {
            Margin = new Thickness(10)
        };

        int classIndex = 0;

        // Add title if set
        if (!string.IsNullOrEmpty(Title))
        {
            var titleBlock = new TextBlock
            {
                Text = Title,
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            formPanel.Children.Add(titleBlock);
        }

        // Process each target object in the collection
        foreach (var target in Targets)
        {
            if (target is null) continue;

            // Subscribe to PropertyChanged events for command refresh
            if (target is INotifyPropertyChanged notifyTarget)
            {
                notifyTarget.PropertyChanged += OnTargetPropertyChanged;
                _subscribedTargets.Add(notifyTarget);
            }

            var descriptor = _effectiveProvider?.GetDescriptor(target.GetType());
            if (descriptor is null) continue;

            var classSectionId = $"class_{classIndex}";
            
            // Create navigation node for this class
            var headerTitle = descriptor.MainHeader ?? descriptor.DisplayName;
            var navNode = new NavigationNode(
                headerTitle,
                null, // icon
                classSectionId,
                descriptor,
                target);
            _navigationNodes.Add(navNode);

            // Create class-level form section
            var classHeader = CreateSectionHeader(headerTitle, classSectionId, true);
            formPanel.Children.Add(classHeader);

            // Create border container for section content
            var contentBorder = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var contentPanel = new StackPanel();

            // Direct properties
            foreach (var prop in descriptor.Properties)
            {
                contentPanel.Children.Add(CreatePropertyControl(prop, target));
            }

            // Subsections
            int subIndex = 0;
            foreach (var subSection in descriptor.SubSections)
            {
                var subSectionId = $"{classSectionId}_sub_{subIndex}";
                
                // Create navigation child node for subsection
                var subNavNode = new NavigationNode(
                    subSection.Title,
                    subIndex,
                    subSectionId,
                    subSection,
                    navNode);
                navNode.Children.Add(subNavNode);

                // Create subsection header
                var subHeader = CreateSectionHeader(subSection.Title, subSectionId, false);
                contentPanel.Children.Add(subHeader);

                foreach (var prop in subSection.Properties)
                {
                    contentPanel.Children.Add(CreatePropertyControl(prop, target));
                }
                
                subIndex++;
            }

            contentBorder.Child = contentPanel;
            formPanel.Children.Add(contentBorder);
            
            classIndex++;
        }

        _formScrollViewer.Content = formPanel;
    }

    private global::Avalonia.Controls.Control CreateSectionHeader(string title, string sectionId, bool isMainHeader)
    {
        var textBlock = new TextBlock 
        { 
            Text = title
        };
        textBlock.Classes.Add(isMainHeader ? "main-header" : "sub-header");
        
        // Store in dictionary for O(1) scroll lookup
        _sectionControlMap[sectionId] = textBlock;
        
        return textBlock;
    }

    private global::Avalonia.Controls.Control CreatePropertyControl(Core.Models.PropertyDescriptor prop, object target)
    {
        var panel = new global::Avalonia.Controls.Grid 
        { 
            Margin = new Thickness(0, 5, 0, 5)
        };
        
        panel.ColumnDefinitions.Add(new global::Avalonia.Controls.ColumnDefinition 
        { 
            Width = new global::Avalonia.Controls.GridLength(150) 
        });
        panel.ColumnDefinitions.Add(new global::Avalonia.Controls.ColumnDefinition 
        { 
            Width = new global::Avalonia.Controls.GridLength(1, global::Avalonia.Controls.GridUnitType.Star) 
        });

        // Label
        var label = new TextBlock 
        { 
            Text = prop.DisplayName
        };
        label.Classes.Add("label");
        global::Avalonia.Controls.Grid.SetColumn(label, 0);
        panel.Children.Add(label);

        // Control
        global::Avalonia.Controls.Control? control = null;

        if (!string.IsNullOrEmpty(prop.CustomControlBinding))
        {
            var controlType = Type.GetType(prop.CustomControlBinding);
            if (controlType != null)
            {
                if (!string.IsNullOrEmpty(prop.CustomControlFactoryMethod))
                {
                    // First, try to find the factory method on the control type (static)
                    var method = controlType.GetMethod(prop.CustomControlFactoryMethod,
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (method != null)
                    {
                        control = method.Invoke(null, null) as global::Avalonia.Controls.Control;
                    }
                    else
                    {
                        // Then, try to find the factory method on the target/settings class (static or instance)
                        var targetType = target.GetType();
                        method = targetType.GetMethod(prop.CustomControlFactoryMethod,
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (method != null)
                        {
                            control = method.Invoke(null, null) as global::Avalonia.Controls.Control;
                        }
                        else
                        {
                            // Try instance method
                            method = targetType.GetMethod(prop.CustomControlFactoryMethod,
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                            if (method != null)
                            {
                                control = method.Invoke(target, null) as global::Avalonia.Controls.Control;
                            }
                        }
                    }
                }

                control ??= Activator.CreateInstance(controlType) as global::Avalonia.Controls.Control;

                if (control != null && !string.IsNullOrEmpty(prop.CustomControlBindingProperty))
                {
                    var propertyField = controlType.GetField(
                        prop.CustomControlBindingProperty + "Property",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
                    if (propertyField?.GetValue(null) is AvaloniaProperty avaloniaProperty)
                    {
                        control.Bind(avaloniaProperty,
                            new Binding(prop.PropertyName) { Source = target, Mode = BindingMode.TwoWay });
                    }
                }
            }
        }

        if (control == null)
        {
            // Handle delegate types (Action, Func, etc.) - create a button
            if (prop.IsDelegate)
            {
                control = CreateDelegateControl(prop, target);
            }
            // Handle collection types
            else if (prop.IsCollection)
            {
                control = CreateCollectionControl(prop, target);
            }
            else if (prop.IsEnum && prop.EnumValues != null)
            {
                var currentValue = _accessor?.GetValue(target, prop.PropertyName)?.ToString();
                var isReadOnly = IsEffectivelyReadOnly(prop, target);
                var comboBox = new ComboBox
                {
                    ItemsSource = prop.EnumValues,
                    SelectedItem = currentValue,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
                    IsEnabled = !isReadOnly
                };
                comboBox.SelectionChanged += (s, e) =>
                {
                    if (comboBox.SelectedItem is string selectedValue && _accessor != null)
                    {
                        var parsedValue = _accessor.GetEnumValue(prop.PropertyTypeName, selectedValue);
                        if (parsedValue != null)
                        {
                            _accessor.SetValue(target, prop.PropertyName, parsedValue);
                        }
                    }
                };

                // Track dynamic ReadOnly controls for refresh
                if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
                {
                    _readOnlyControls.Add((comboBox, () => !IsEffectivelyReadOnly(prop, target)));
                }

                control = comboBox;
            }
            else if (prop.PropertyTypeName == "System.Boolean" || prop.PropertyTypeName == "bool")
            {
                var currentValue = _accessor?.GetValue(target, prop.PropertyName);
                var isReadOnly = IsEffectivelyReadOnly(prop, target);
                var toggleSwitch = new ToggleSwitch
                {
                    IsChecked = currentValue is bool b ? b : false,
                    Content = prop.DisplayName,
                    IsEnabled = !isReadOnly
                };
                toggleSwitch.IsCheckedChanged += (s, e) =>
                {
                    _accessor?.SetValue(target, prop.PropertyName, toggleSwitch.IsChecked ?? false);
                };

                // Track dynamic ReadOnly controls for refresh
                if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
                {
                    _readOnlyControls.Add((toggleSwitch, () => !IsEffectivelyReadOnly(prop, target)));
                }

                control = toggleSwitch;
            }
            else
            {
                control = CreateTextBoxWithFeatures(prop, target);
            }
        }

        if (control != null)
        {
            ApplyControlAttributes(control, prop, target);
            global::Avalonia.Controls.Grid.SetColumn(control, 1);
            panel.Children.Add(control);
        }

        return panel;
    }

    #region Delegate (Command) Controls

    /// <summary>
    /// Creates a button control for delegate properties (Action, Func, etc.).
    /// </summary>
    private global::Avalonia.Controls.Control CreateDelegateControl(Core.Models.PropertyDescriptor prop, object target)
    {
        var button = new Button
        {
            Content = prop.DisplayName,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Thickness(15, 5, 15, 5),
            MinWidth = 80
        };

        // Get the delegate value
        var delegateValue = _accessor?.GetValue(target, prop.PropertyName) as Delegate;

        // Use Click event instead of Command for better async support
        button.Click += (s, e) =>
        {
            if (delegateValue != null)
            {
                delegateValue.DynamicInvoke();
            }
        };

        // Determine if button has dynamic CanExecute condition
        var hasCanExecuteCondition = !string.IsNullOrEmpty(prop.CanExecuteMethodName) ||
                                      prop.IsReadOnlyDynamic ||
                                      prop.IsReadOnly;

        if (hasCanExecuteCondition)
        {
            // Set initial enabled state
            button.IsEnabled = CanExecuteDelegate(prop, target);

            // Track button for dynamic IsEnabled updates (similar to ReadOnly controls)
            _readOnlyControls.Add((button, () => !CanExecuteDelegate(prop, target)));
        }

        return button;
    }

    /// <summary>
    /// Determines if a delegate is async (returns Task).
    /// </summary>
    private bool IsAsyncDelegate(Core.Models.PropertyDescriptor prop, Delegate? delegateValue)
    {
        if (delegateValue == null) return false;
        
        var method = delegateValue.Method;
        return method.ReturnType == typeof(Task) || 
               method.ReturnType == typeof(ValueTask) ||
               (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }

    /// <summary>
    /// Executes a delegate, handling async delegates properly.
    /// </summary>
    private void ExecuteDelegate(Delegate? delegateValue, bool isAsync)
    {
        if (delegateValue == null) return;

        if (isAsync)
        {
            // Execute async and handle exceptions
            _ = ExecuteAsync(delegateValue);
        }
        else
        {
            delegateValue.DynamicInvoke();
        }
    }

    /// <summary>
    /// Executes an async delegate.
    /// </summary>
    private async Task ExecuteAsync(Delegate delegateValue)
    {
        try
        {
            var result = delegateValue.DynamicInvoke();
            if (result is Task task)
            {
                await task;
            }
            else if (result is ValueTask valueTask)
            {
                await valueTask;
            }
        }
        catch (Exception ex)
        {
            // Log or handle exception
            System.Diagnostics.Debug.WriteLine($"Async delegate error: {ex}");
        }
    }

    /// <summary>
    /// Determines if a delegate command can execute.
    /// </summary>
    private bool CanExecuteDelegate(Core.Models.PropertyDescriptor prop, object target)
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

    #region Attribute Helpers

    /// <summary>
    /// Gets the PropertyInfo for a property from the target object.
    /// </summary>
    private PropertyInfo? GetPropertyInfo(object target, string propertyName)
    {
        return target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Applies layout, placeholder, and description attributes to a control.
    /// </summary>
    private void ApplyControlAttributes(global::Avalonia.Controls.Control control, Core.Models.PropertyDescriptor prop, object target)
    {
        var propertyInfo = GetPropertyInfo(target, prop.PropertyName);
        if (propertyInfo == null) return;

        // Apply layout attributes
        control.ApplyLayout(propertyInfo);

        // Apply placeholder to TextBox
        if (control is TextBox textBox)
        {
            textBox.ApplyPlaceholder(propertyInfo);
        }

        // Apply description (tooltip)
        control.ApplyDescription(propertyInfo);
    }

    /// <summary>
    /// Creates a TextBox with password support and validation.
    /// </summary>
    private global::Avalonia.Controls.Control CreateTextBoxWithFeatures(Core.Models.PropertyDescriptor prop, object target)
    {
        var isReadOnly = IsEffectivelyReadOnly(prop, target);
        var propertyInfo = GetPropertyInfo(target, prop.PropertyName);
        
        // Check for PasswordAttribute
        var passwordAttr = propertyInfo?.GetCustomAttribute<PasswordAttribute>();
        var isPassword = passwordAttr != null || 
            prop.PropertyName.ToLowerInvariant().Contains("password") ||
            prop.PropertyName.ToLowerInvariant().Contains("pwd");
        
        var textBox = new TextBox
        {
            Text = _accessor?.GetValue(target, prop.PropertyName)?.ToString() ?? "",
            IsReadOnly = isReadOnly
        };

        // Apply password masking if needed
        if (isPassword)
        {
            textBox.PasswordChar = passwordAttr?.MaskChar ?? '•';
        }

        // Track dynamic ReadOnly controls for refresh
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            _readOnlyControls.Add((textBox, () => IsEffectivelyReadOnly(prop, target)));
        }

        // Helper function to convert and set value
        void SetConvertedValue(string? textValue)
        {
            try
            {
                var propType = propertyInfo?.PropertyType ?? Type.GetType(prop.PropertyTypeName);
                if (propType == null) return;

                if (propType == typeof(string))
                {
                    _accessor?.SetValue(target, prop.PropertyName, textValue ?? "");
                }
                else if (!string.IsNullOrEmpty(textValue))
                {
                    var convertedValue = Convert.ChangeType(textValue, propType);
                    _accessor?.SetValue(target, prop.PropertyName, convertedValue);
                }
            }
            catch { /* Ignore conversion errors */ }
        }

        // Add validation if attributes exist
        if (propertyInfo != null && AutoSettingUI.Avalonia.ControlLayoutHelper.GetValidations(propertyInfo).Length > 0)
        {
            textBox.TextChanged += (s, e) =>
            {
                var newValue = textBox.Text;
                if (AutoSettingUI.Avalonia.ControlLayoutHelper.ValidateAndUpdateVisual(textBox, newValue, propertyInfo, target))
                {
                    SetConvertedValue(newValue);
                }
            };
        }
        else
        {
            textBox.TextChanged += (s, e) =>
            {
                SetConvertedValue(textBox.Text);
            };
        }

        return textBox;
    }

    #endregion

    #region Collection Controls

    /// <summary>
    /// Creates a control for editing collection properties.
    /// </summary>
    private global::Avalonia.Controls.Control CreateCollectionControl(Core.Models.PropertyDescriptor prop, object target)
    {
        // Default collection editor
        return CreateDefaultCollectionEditor(prop, target);
    }

    /// <summary>
    /// Creates a default collection editor with add/remove/reorder capabilities.
    /// </summary>
    private global::Avalonia.Controls.Control CreateDefaultCollectionEditor(Core.Models.PropertyDescriptor prop, object target)
    {
        var elementType = GetElementTypeFromName(prop.CollectionElementTypeName);
        var collection = _accessor?.GetValue(target, prop.PropertyName) as IList;
        if (collection == null)
        {
            // Try to create a new list if the property is null
            var propInfo = target.GetType().GetProperty(prop.PropertyName);
            if (propInfo != null && propInfo.CanWrite)
            {
                var listType = typeof(List<>).MakeGenericType(elementType ?? typeof(object));
                collection = (IList?)Activator.CreateInstance(listType);
                propInfo.SetValue(target, collection);
            }
        }

        var panel = new StackPanel();

        // Items list
        var listBox = new ListBox
        {
            MinHeight = 60,
            MaxHeight = 200,
            Margin = new Thickness(0, 0, 0, 5),
            ItemsSource = collection as IEnumerable
        };

        if (elementType != null)
        {
            listBox.ItemTemplate = CreateCollectionItemTemplate(elementType);
        }

        panel.Children.Add(listBox);

        // Button panel
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5
        };

        // Add button
        if (prop.CollectionAllowAdd)
        {
            var addBtn = new Button
            {
                Content = "+",
                Width = 30,
                Height = 30,
                MinWidth = 0,
                MinHeight = 0,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            ToolTip.SetTip(addBtn, "Add item");
            addBtn.Click += (s, e) =>
            {
                if (collection != null && elementType != null)
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
                            newItem = "";
                        }
                        else if (elementType.GetConstructor(Type.EmptyTypes) != null)
                        {
                            newItem = Activator.CreateInstance(elementType);
                        }
                        else
                        {
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
                            else newItem = null;
                        }

                        if (newItem != null || elementType.IsValueType)
                        {
                            collection.Add(newItem);
                            // Refresh
                            listBox.ItemsSource = null;
                            listBox.ItemsSource = collection as IEnumerable;
                        }
                    }
                    catch { }
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
                Width = 30,
                Height = 30,
                MinWidth = 0,
                MinHeight = 0,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            ToolTip.SetTip(removeBtn, "Remove selected item");
            removeBtn.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null && collection != null)
                {
                    collection.Remove(listBox.SelectedItem);
                    listBox.ItemsSource = null;
                    listBox.ItemsSource = collection as IEnumerable;
                }
            };
            buttonPanel.Children.Add(removeBtn);
        }

        // Move up/down buttons
        if (prop.CollectionAllowReorder)
        {
            var upBtn = new Button 
            { 
                Content = "↑", 
                Width = 30, 
                Height = 30,
                MinWidth = 0,
                MinHeight = 0,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            ToolTip.SetTip(upBtn, "Move up");
            upBtn.Click += (s, e) =>
            {
                var index = listBox.SelectedIndex;
                if (index > 0 && collection != null)
                {
                    var item = collection[index];
                    collection.RemoveAt(index);
                    collection.Insert(index - 1, item);
                    listBox.SelectedIndex = index - 1;
                    listBox.ItemsSource = null;
                    listBox.ItemsSource = collection as IEnumerable;
                }
            };
            buttonPanel.Children.Add(upBtn);

            var downBtn = new Button 
            { 
                Content = "↓", 
                Width = 30, 
                Height = 30,
                MinWidth = 0,
                MinHeight = 0,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Padding = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            ToolTip.SetTip(downBtn, "Move down");
            downBtn.Click += (s, e) =>
            {
                var index = listBox.SelectedIndex;
                if (index >= 0 && index < collection!.Count - 1)
                {
                    var item = collection[index];
                    collection.RemoveAt(index);
                    collection.Insert(index + 1, item);
                    listBox.SelectedIndex = index + 1;
                    listBox.ItemsSource = null;
                    listBox.ItemsSource = collection as IEnumerable;
                }
            };
            buttonPanel.Children.Add(downBtn);
        }

        panel.Children.Add(buttonPanel);

        return panel;
    }

    private IDataTemplate CreateCollectionItemTemplate(Type elementType)
    {
        return new FuncDataTemplate<object>((item, scope) =>
        {
            if (item == null) return new TextBlock { Text = "Null" };

            // Simple types
            if (elementType == typeof(string) || elementType == typeof(int) || elementType == typeof(double) ||
                elementType == typeof(float) || elementType == typeof(decimal) || elementType == typeof(bool))
            {
                var tb = new TextBox { HorizontalAlignment = HorizontalAlignment.Stretch };
                tb.Bind(TextBox.TextProperty, new Binding(".") { Mode = BindingMode.TwoWay });
                return tb;
            }

            // Complex types
            var panel = new StackPanel { Spacing = 5, Margin = new Thickness(5) };
            var props = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var p in props)
            {
                var row = new global::Avalonia.Controls.Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(100)));
                row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                var label = new TextBlock { Text = p.Name + ":", VerticalAlignment = VerticalAlignment.Center };
                global::Avalonia.Controls.Grid.SetColumn(label, 0);

                global::Avalonia.Controls.Control editor;
                if (p.PropertyType == typeof(bool))
                {
                    var ts = new ToggleSwitch();
                    ts.Bind(ToggleSwitch.IsCheckedProperty, new Binding(p.Name) { Mode = BindingMode.TwoWay });
                    editor = ts;
                }
                else
                {
                    var tb = new TextBox { HorizontalAlignment = HorizontalAlignment.Stretch };
                    tb.Bind(TextBox.TextProperty, new Binding(p.Name) { Mode = BindingMode.TwoWay });
                    editor = tb;
                }

                global::Avalonia.Controls.Grid.SetColumn(editor, 1);
                row.Children.Add(label);
                row.Children.Add(editor);
                panel.Children.Add(row);
            }

            return new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Child = panel
            };
        }, true);
    }

    #endregion

    #region ReadOnly Support

    /// <summary>
    /// Determines if a property is effectively read-only.
    /// </summary>
    private bool IsEffectivelyReadOnly(Core.Models.PropertyDescriptor prop, object target)
    {
        // If dynamic method is specified, call that
        if (prop.IsReadOnlyDynamic && !string.IsNullOrEmpty(prop.ReadOnlyMethodName))
        {
            var method = target.GetType().GetMethod(prop.ReadOnlyMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (method != null)
            {
                var result = method.IsStatic
                    ? method.Invoke(null, null)
                    : method.Invoke(target, null);
                return result is bool b && b;
            }
        }

        return prop.IsReadOnly;
    }

    /// <summary>
    /// Gets a type from its name, searching in all loaded assemblies.
    /// </summary>
    private Type? GetElementTypeFromName(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName))
            return null;

        // Try direct type resolution first
        var type = Type.GetType(typeName);
        if (type is not null)
            return type;

        // Common type fallbacks
        type = typeName.ToLowerInvariant() switch
        {
            "string" => typeof(string),
            "system.string" => typeof(string),
            "int" => typeof(int),
            "system.int32" => typeof(int),
            "bool" => typeof(bool),
            "system.boolean" => typeof(bool),
            "double" => typeof(double),
            "system.double" => typeof(double),
            "float" => typeof(float),
            "system.single" => typeof(float),
            _ => null
        };
        if (type is not null) return type;

        // Search in all loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName);
            if (type is not null)
                return type;
        }

        return null;
    }

    #endregion
}

/// <summary>
/// A simple delegate command implementation for button bindings.
/// </summary>
internal class DelegateCommand : global::System.Windows.Input.ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
