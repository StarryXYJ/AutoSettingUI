using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoSettingUI.Core.Interfaces;
using AutoSettingUI.Core.Models;
using AutoSettingUI.Core.Providers;
using AutoSettingUI.WPF.Factories;

namespace AutoSettingUI.WPF.Controls;

/// <summary>
/// WPF implementation of the AutoSettingPanel control.
/// </summary>
public class WpfAutoSettingPanel : Control
{
    static WpfAutoSettingPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfAutoSettingPanel),
            new FrameworkPropertyMetadata(typeof(WpfAutoSettingPanel)));
    }

     #region Dependency Properties

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(WpfAutoSettingPanel), new PropertyMetadata(null));

    public static readonly DependencyProperty TargetsProperty = DependencyProperty.Register(
        nameof(Targets), typeof(IList), typeof(WpfAutoSettingPanel), 
        new PropertyMetadata(null, OnTargetsChanged));

    public static readonly DependencyProperty DescriptorProviderProperty = DependencyProperty.Register(
        nameof(DescriptorProvider), typeof(ISettingDescriptorProvider), typeof(WpfAutoSettingPanel),
        new PropertyMetadata(null, OnDescriptorProviderChanged));

    public static readonly DependencyProperty ShowNavigationProperty = DependencyProperty.Register(
        nameof(ShowNavigation), typeof(bool), typeof(WpfAutoSettingPanel), new PropertyMetadata(true));

    public static readonly DependencyProperty NavigationWidthProperty = DependencyProperty.Register(
        nameof(NavigationWidth), typeof(double), typeof(WpfAutoSettingPanel), new PropertyMetadata(200.0));

    #endregion

    #region Properties

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of target objects to display.
    /// </summary>
    public IList? Targets
    {
        get => (IList?)GetValue(TargetsProperty);
        set => SetValue(TargetsProperty, value);
    }

    [Obsolete("Use Targets property instead")]
    public object? CurrentTarget
    {
        get => Targets?.Cast<object?>().FirstOrDefault();
        set => Targets = value is not null ? new[] { value } : null;
    }

    public ISettingDescriptorProvider? DescriptorProvider
    {
        get => (ISettingDescriptorProvider?)GetValue(DescriptorProviderProperty);
        set => SetValue(DescriptorProviderProperty, value);
    }

    public bool ShowNavigation
    {
        get => (bool)GetValue(ShowNavigationProperty);
        set => SetValue(ShowNavigationProperty, value);
    }

    public double NavigationWidth
    {
        get => (double)GetValue(NavigationWidthProperty);
        set => SetValue(NavigationWidthProperty, value);
    }

    #endregion

    private ScrollViewer? _formScrollViewer;
    private TreeView? _navigationTree;
    private ISettingDescriptorProvider? _effectiveProvider;
    private readonly List<FormSection> _formSections = new();
    private readonly ObservableCollection<NavigationNode> _navigationNodes = new();
    private IEnumerable? _previousTargets;

    public WpfAutoSettingPanel()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeProvider();
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

    private static void OnTargetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfAutoSettingPanel panel)
        {
            // Unsubscribe from old collection changes
            if (panel._previousTargets is INotifyCollectionChanged oldNotify)
            {
                oldNotify.CollectionChanged -= panel.OnTargetsCollectionChanged;
            }
            
            // Subscribe to new collection changes
            if (e.NewValue is INotifyCollectionChanged newNotify)
            {
                newNotify.CollectionChanged += panel.OnTargetsCollectionChanged;
            }
            panel._previousTargets = e.NewValue as IEnumerable;
            
            // Ensure provider is initialized
            panel.InitializeProvider();
            
            // Register all new target types
            if (panel._effectiveProvider is not null && e.NewValue is IEnumerable enumerable)
            {
                var provider = panel._effectiveProvider as ReflectionSettingDescriptorProvider;
                foreach (var target in enumerable)
                {
                    if (target is not null)
                    {
                        provider?.RegisterType(target.GetType());
                    }
                }
            }
            
            // Build UI if template is applied
            if (panel._formScrollViewer is not null)
            {
                panel.BuildUI();
            }
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

    private static void OnDescriptorProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfAutoSettingPanel panel && e.NewValue is ISettingDescriptorProvider provider)
        {
            panel._effectiveProvider = provider;
            panel.BuildUI();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        _formScrollViewer = GetTemplateChild("PART_FormScrollViewer") as ScrollViewer;
        _navigationTree = GetTemplateChild("PART_Navigation") as TreeView;
        
        if (_navigationTree is not null)
        {
            _navigationTree.SelectedItemChanged += OnNavigationItemSelected;
        }
        
        // Initialize provider if not already done
        InitializeProvider();
        
        // Build UI now that template is applied
        if (_effectiveProvider is not null && Targets is not null && _formScrollViewer is not null)
        {
            BuildUI();
        }
    }

    private void OnNavigationItemSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is NavigationNode node && _formScrollViewer is not null)
        {
            // Find the corresponding section element and scroll to it
            ScrollToSection(node.SectionId);
        }
    }

    private void ScrollToSection(string sectionId)
    {
        if (_formScrollViewer is null) return;
        
        // Find the element with the matching tag
        var element = FindChildByTag(_formScrollViewer, sectionId);
        if (element is not null)
        {
            element.BringIntoView();
        }
    }

    private static FrameworkElement? FindChildByTag(DependencyObject parent, string tag)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is FrameworkElement element && element.Tag?.ToString() == tag)
            {
                return element;
            }
            
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
        if (_formScrollViewer is null || Targets is null || _effectiveProvider is null) return;

        // Clear previous data
        _formSections.Clear();
        
        // Clear navigation nodes - do this before setting ItemsSource if not already set
        _navigationNodes.Clear();
        
        // Set ItemsSource once on first build
        if (_navigationTree is not null && _navigationTree.ItemsSource != _navigationNodes)
        {
            _navigationTree.ItemsSource = _navigationNodes;
        }
        
        var formPanel = new StackPanel { Margin = new Thickness(15) };
        int classIndex = 0;

        // Title
        if (!string.IsNullOrEmpty(Title))
        {
            formPanel.Children.Add(new TextBlock 
            { 
                Text = Title, 
                FontSize = 24, 
                FontWeight = FontWeights.Bold, 
                Margin = new Thickness(0, 0, 0, 20) 
            });
        }

        // Process each target object in the collection
        foreach (var target in Targets)
        {
            if (target is null) continue;
            
            var descriptor = _effectiveProvider.GetDescriptor(target.GetType());
            if (descriptor is null) continue;

            // Create factory with class descriptor for default factory support
            var factory = new WpfControlFactory(descriptor);

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

            // Direct properties
            foreach (var prop in descriptor.Properties)
            {
                formPanel.Children.Add(CreatePropertyControl(factory, prop, target));
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
                formPanel.Children.Add(subHeader);

                foreach (var prop in subSection.Properties)
                {
                    formPanel.Children.Add(CreatePropertyControl(factory, prop, target));
                }
                
                subIndex++;
            }
            
            classIndex++;
        }

        _formScrollViewer.Content = formPanel;
    }

    private static FrameworkElement CreateSectionHeader(string title, string sectionId, bool isMainHeader)
    {
        var textBlock = new TextBlock 
        { 
            Text = title, 
            FontWeight = isMainHeader ? FontWeights.SemiBold : FontWeights.Normal,
            Margin = isMainHeader ? new Thickness(0, 20, 0, 10) : new Thickness(0, 15, 0, 5)
        };
        
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

    private UIElement CreatePropertyControl(WpfControlFactory factory, PropertyDescriptor prop, object target)
    {
        var panel = new Grid { Margin = new Thickness(0, 5, 0, 5) };
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Label
        var label = new TextBlock 
        { 
            Text = prop.DisplayName, 
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        Grid.SetColumn(label, 0);
        panel.Children.Add(label);

        // Control
        var control = factory.CreateControl(prop, target);
        Grid.SetColumn(control, 1);
        panel.Children.Add(control);

        return panel;
    }
}

/// <summary>
/// Navigation item for the sidebar.
/// </summary>
public class NavigationItem
{
    public string Title { get; }
    public bool IsMainHeader { get; }

    public NavigationItem(string title, bool isMainHeader)
    {
        Title = title;
        IsMainHeader = isMainHeader;
    }
}

/// <summary>
/// Converts boolean to FontWeight.
/// </summary>
public class BooleanToFontWeightConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool b && b)
            return FontWeights.Bold;
        return FontWeights.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
