using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.Core.Providers;

namespace AutoSettingUI.Ursa.Controls;

/// <summary>
/// Ursa implementation of the AutoSettingPanel control with CardBorder theming.
/// </summary>
public class UrsaAutoSettingPanel : TemplatedControl
{
    private ScrollViewer? _formScrollViewer;
    private global::Avalonia.Controls.TreeView? _navigationTree;
    private ISettingDescriptorProvider? _effectiveProvider;
    private readonly List<FormSection> _formSections = new();
    private readonly ObservableCollection<NavigationNode> _navigationNodes = new();
    private IEnumerable? _previousTargets;

    #region Styled Properties

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, string?>(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Targets"/> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> TargetsProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, IEnumerable?>(nameof(Targets));

    /// <summary>
    /// Defines the <see cref="DescriptorProvider"/> property.
    /// </summary>
    public static readonly StyledProperty<ISettingDescriptorProvider?> DescriptorProviderProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, ISettingDescriptorProvider?>(nameof(DescriptorProvider));

    /// <summary>
    /// Defines the <see cref="ShowNavigation"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowNavigationProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, bool>(nameof(ShowNavigation), true);

    /// <summary>
    /// Defines the <see cref="NavigationWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> NavigationWidthProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, double>(nameof(NavigationWidth), 220);

    /// <summary>
    /// Defines the <see cref="UseCardBorderTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> UseCardBorderThemeProperty =
        AvaloniaProperty.Register<UrsaAutoSettingPanel, bool>(nameof(UseCardBorderTheme), true);

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
    /// Gets or sets a value indicating whether to use CardBorder theme for sections.
    /// </summary>
    public bool UseCardBorderTheme
    {
        get => GetValue(UseCardBorderThemeProperty);
        set => SetValue(UseCardBorderThemeProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="UrsaAutoSettingPanel"/> class.
    /// </summary>
    public UrsaAutoSettingPanel()
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
        // Register new types if items were added
        if (e.NewItems is not null && _effectiveProvider is not null)
        {
            var provider = _effectiveProvider as ReflectionSettingDescriptorProvider;
            foreach (var item in e.NewItems)
            {
                if (item is not null)
                {
                    provider?.RegisterType(item.GetType());
                }
            }
        }
        
        // Rebuild UI
        if (_formScrollViewer is not null)
        {
            BuildUI();
        }
    }

    private void InitializeProvider()
    {
        if (_effectiveProvider is not null) return;

        // Use reflection-based provider as fallback
        _effectiveProvider = new ReflectionSettingDescriptorProvider();
        
        // Register all target types
        if (Targets is not null)
        {
            foreach (var target in Targets)
            {
                if (target is not null)
                {
                    ((ReflectionSettingDescriptorProvider)_effectiveProvider).RegisterType(target.GetType());
                }
            }
        }
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
        
        // Find the element with the matching tag
        var element = FindChildByTag(_formScrollViewer, sectionId);
        if (element is global::Avalonia.Controls.Control control)
        {
            control.BringIntoView();
        }
    }

    private static global::Avalonia.Visual? FindChildByTag(global::Avalonia.Visual parent, string tag)
    {
        if (parent is global::Avalonia.Controls.Control control && control.Tag?.ToString() == tag)
        {
            return control;
        }

        var children = parent.GetVisualChildren();
        foreach (var child in children)
        {
            var result = FindChildByTag(child, tag);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
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

        var formPanel = new StackPanel
        {
            Margin = new Thickness(15)
        };

        int classIndex = 0;

        // Add title if set
        if (!string.IsNullOrEmpty(Title))
        {
            var titleBlock = new TextBlock
            {
                Text = Title,
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 0, 0, 25)
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
                descriptor.Order,
                classSectionId,
                descriptor,
                target);
            _navigationNodes.Add(navNode);

            // Create class-level form section
            var classHeader = CreateSectionHeader(headerTitle, classSectionId, true);
            formPanel.Children.Add(classHeader);

            // Add CardBorder container for section content
            var contentCard = new Border
            {
                Margin = new Thickness(0, 0, 0, 20),
                Padding = new Thickness(20),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };

            var contentPanel = new StackPanel
            {
                Spacing = 5
            };

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

            contentCard.Child = contentPanel;
            formPanel.Children.Add(contentCard);
            
            classIndex++;
        }

        _formScrollViewer.Content = formPanel;
    }

    private static global::Avalonia.Controls.Control CreateSectionHeader(string title, string sectionId, bool isMainHeader)
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
        
        // Set the tag for scrolling
        textBlock.Tag = sectionId;
        
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
        var propertyInfo = target.GetType().GetProperty(prop.PropertyName);

        if (!string.IsNullOrEmpty(prop.CustomControlBinding))
        {
            var controlType = Type.GetType(prop.CustomControlBinding);
            if (controlType != null)
            {
                if (!string.IsNullOrEmpty(prop.CustomControlFactoryMethod))
                {
                    var method = controlType.GetMethod(prop.CustomControlFactoryMethod, BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        control = method.Invoke(null, null) as global::Avalonia.Controls.Control;
                    }
                }
                
                if (control == null)
                {
                    control = Activator.CreateInstance(controlType) as global::Avalonia.Controls.Control;
                }

                if (control != null && !string.IsNullOrEmpty(prop.CustomControlBindingProperty))
                {
                    var propertyField = controlType.GetField(prop.CustomControlBindingProperty + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    if (propertyField != null && propertyField.GetValue(null) is AvaloniaProperty avaloniaProperty)
                    {
                        control.Bind(avaloniaProperty, new Binding(prop.PropertyName) { Source = target, Mode = BindingMode.TwoWay });
                    }
                }
            }
        }

        if (control == null)
        {
            if (prop.IsEnum && propertyInfo != null)
            {
                var comboBox = new ComboBox
                {
                    ItemsSource = Enum.GetValues(propertyInfo.PropertyType),
                    SelectedItem = propertyInfo.GetValue(target),
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch
                };
                comboBox.SelectionChanged += (s, e) =>
                {
                    if (comboBox.SelectedItem != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(target, comboBox.SelectedItem);
                    }
                };
                control = comboBox;
            }
            else if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
            {
                var checkBox = new global::Avalonia.Controls.Primitives.ToggleButton
                {
                    IsChecked = (bool?)propertyInfo.GetValue(target),
                    Content = prop.DisplayName,
                    HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Left
                };
                checkBox.IsCheckedChanged += (s, e) =>
                {
                    if (propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(target, checkBox.IsChecked ?? false);
                    }
                };
                control = checkBox;
            }
            else
            {
                var textBox = new TextBox 
                { 
                    Text = GetPropertyValue(target, prop.PropertyName)?.ToString() ?? ""
                };
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    textBox.TextChanged += (s, e) => 
                    {
                        try
                        {
                            propertyInfo.SetValue(target, Convert.ChangeType(textBox.Text, propertyInfo.PropertyType));
                        } catch { }
                    };
                }
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

    private static object? GetPropertyValue(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        return property?.GetValue(target);
    }
}
