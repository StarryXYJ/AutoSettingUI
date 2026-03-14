using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.Core.Providers;

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

        // Clear previous data
        _formSections.Clear();
        _navigationNodes.Clear();
        _sectionControlMap.Clear();

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
            Text = title, 
            FontWeight = isMainHeader ? FontWeight.SemiBold : FontWeight.Normal,
            Margin = isMainHeader ? new Thickness(0, 20, 0, 10) : new Thickness(0, 15, 0, 5)
        };
        textBlock.Classes.Add(isMainHeader ? "main-header" : "sub-header");
        
        if (isMainHeader)
        {
            textBlock.FontSize = 20;
        }
        else
        {
            textBlock.FontSize = 16;
            textBlock.Foreground = Brushes.Gray;
        }
        
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
            Text = prop.DisplayName, 
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
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
            if (prop.IsEnum && prop.EnumValues != null)
            {
                var currentValue = _accessor?.GetValue(target, prop.PropertyName)?.ToString();
                var comboBox = new ComboBox
                {
                    ItemsSource = prop.EnumValues,
                    SelectedItem = currentValue,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch
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
                control = comboBox;
            }
            else if (prop.PropertyTypeName == "System.Boolean" || prop.PropertyTypeName == "bool")
            {
                var currentValue = _accessor?.GetValue(target, prop.PropertyName);
                var checkBox = new global::Avalonia.Controls.Primitives.ToggleButton
                {
                    IsChecked = currentValue is bool b ? b : false,
                    Content = prop.DisplayName,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left
                };
                checkBox.IsCheckedChanged += (s, e) =>
                {
                    _accessor?.SetValue(target, prop.PropertyName, checkBox.IsChecked ?? false);
                };
                control = checkBox;
            }
            else
            {
                var textBox = new TextBox
                {
                    Text = _accessor?.GetValue(target, prop.PropertyName)?.ToString() ?? ""
                };
                textBox.TextChanged += (s, e) =>
                {
                    try
                    {
                        var propType = Type.GetType(prop.PropertyTypeName);
                        if (propType != null)
                            _accessor?.SetValue(target, prop.PropertyName,
                                Convert.ChangeType(textBox.Text, propType));
                    }
                    catch { }
                };
                control = textBox;
            }
        }

        if (control != null)
        {
            global::Avalonia.Controls.Grid.SetColumn(control, 1);
            panel.Children.Add(control);
        }

        return panel;
    }
}
