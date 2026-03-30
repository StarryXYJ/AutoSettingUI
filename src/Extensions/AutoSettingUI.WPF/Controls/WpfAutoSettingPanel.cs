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
using DynamicLocalization.Core;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;

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

    public static readonly DependencyProperty PropertyAccessorProperty = DependencyProperty.Register(
        nameof(PropertyAccessor), typeof(IPropertyValueAccessor), typeof(WpfAutoSettingPanel),
        new PropertyMetadata(null, OnPropertyAccessorChanged));

    public static readonly DependencyProperty LocalizationServiceProperty = DependencyProperty.Register(
        nameof(LocalizationService), typeof(ICultureService), typeof(WpfAutoSettingPanel),
        new PropertyMetadata(null, OnLocalizationServiceChanged));

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

    /// <summary>
    /// Gets or sets the property accessor used to get and set values on target objects.
    /// Inject the source-generated <c>GeneratedSettingProvider</c> here for AOT compatibility.
    /// Falls back to reflection if not set.
    /// </summary>
    public IPropertyValueAccessor? PropertyAccessor
    {
        get => (IPropertyValueAccessor?)GetValue(PropertyAccessorProperty);
        set => SetValue(PropertyAccessorProperty, value);
    }

    /// <summary>
    /// Gets or sets the localization service for dynamic language switching.
    /// When set, all text in the panel will be automatically updated when the culture changes.
    /// </summary>
    public ICultureService? LocalizationService
    {
        get => (ICultureService?)GetValue(LocalizationServiceProperty);
        set => SetValue(LocalizationServiceProperty, value);
    }

    #endregion

    private ScrollViewer? _formScrollViewer;
    private TreeView? _navigationTree;
    private ISettingDescriptorProvider? _effectiveProvider;
    private readonly List<FormSection> _formSections = new();
    private readonly ObservableCollection<NavigationNode> _navigationNodes = new();
    private readonly Dictionary<string, FrameworkElement> _sectionControlMap = new();
    private IEnumerable? _previousTargets;
    private readonly List<(FrameworkElement Control, Func<bool> IsReadOnlyGetter)> _readOnlyControls = new();
    private readonly List<(FrameworkElement Control, Func<bool> IsVisibleGetter)> _visibilityControls = new();
    private readonly List<(TextBlock TextBlock, string? ResourceKey, string FallbackText)> _localizedTextBlocks = new();
    private readonly List<(FrameworkElement Control, string? PlaceholderKey, string? PlaceholderFallback)> _localizedPlaceholders = new();
    private readonly HashSet<INotifyPropertyChanged> _subscribedTargets = new();
    private ICultureService? _localizationService;

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

        // Try to use the source-generated provider first (AOT-compatible)
        // The generator creates AutoSettingUI.Generated.GeneratedSettingProvider
        var generatedAccessor = TryCreateGeneratedProvider();
        if (generatedAccessor is not null)
        {
            _effectiveProvider = generatedAccessor;
            return;
        }

        // Fallback to reflection-based provider (non-AOT)
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

    /// <summary>
    /// Tries to create the source-generated provider via reflection.
    /// This allows AOT-compatible usage without explicit reference to generated code.
    /// </summary>
    private static ISettingDescriptorProvider? TryCreateGeneratedProvider()
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
                    if (instance is ISettingDescriptorProvider provider)
                        return provider;
                }
            }
        }
        catch
        {
            // Ignore errors and fall back to reflection
        }
        return null;
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
            
            // Unsubscribe from PropertyChanged events on old targets
            panel.UnsubscribeFromPropertyChangedEvents();
            
            // Subscribe to new collection changes
            if (e.NewValue is INotifyCollectionChanged newNotify)
            {
                newNotify.CollectionChanged += panel.OnTargetsCollectionChanged;
            }
            panel._previousTargets = e.NewValue as IEnumerable;
            
            // Ensure provider is initialized
            panel.InitializeProvider();
            
            // Register all new target types and subscribe to PropertyChanged events
            if (panel._effectiveProvider is not null && e.NewValue is IEnumerable enumerable)
            {
                var provider = panel._effectiveProvider as ReflectionSettingDescriptorProvider;
                foreach (var target in enumerable)
                {
                    if (target is not null)
                    {
                        provider?.RegisterType(target.GetType());
                        panel.SubscribeToPropertyChangedEvents(target);
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
        // Unsubscribe from removed items
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged && _subscribedTargets.Contains(notifyPropertyChanged))
                {
                    notifyPropertyChanged.PropertyChanged -= OnTargetPropertyChanged;
                    _subscribedTargets.Remove(notifyPropertyChanged);
                }
            }
        }
        
        // Register new types and subscribe to PropertyChanged events if items were added
        if (e.NewItems is not null && _effectiveProvider is not null)
        {
            var provider = _effectiveProvider as ReflectionSettingDescriptorProvider;
            foreach (var item in e.NewItems)
            {
                if (item is not null)
                {
                    provider?.RegisterType(item.GetType());
                    SubscribeToPropertyChangedEvents(item);
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

    private static void OnPropertyAccessorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfAutoSettingPanel panel && e.NewValue is IPropertyValueAccessor)
        {
            panel.BuildUI();
        }
    }

    private static void OnLocalizationServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WpfAutoSettingPanel panel)
        {
            if (e.OldValue is ICultureService oldService)
            {
                oldService.CultureChanged -= panel.OnCultureChanged;
                oldService.PropertyChanged -= panel.OnLocalizationServicePropertyChanged;
            }
            
            if (e.NewValue is ICultureService newService)
            {
                newService.CultureChanged += panel.OnCultureChanged;
                newService.PropertyChanged += panel.OnLocalizationServicePropertyChanged;
            }
            
            panel._localizationService = e.NewValue as ICultureService;
            panel.UpdateLocalizedTexts();
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

    /// <summary>
    /// Refreshes all dynamic states (IsEnabled for buttons with CanExecute, dynamic ReadOnly states).
    /// Call this method when underlying data changes that affects CanExecute or ReadOnly conditions.
    /// </summary>
    public void Refresh()
    {
        // Update IsEnabled/IsReadOnly state for controls with dynamic conditions
        foreach (var (control, isReadOnlyGetter) in _readOnlyControls)
        {
            var isReadOnly = isReadOnlyGetter();
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
            else if (control is Slider slider)
            {
                slider.IsEnabled = !isReadOnly;
            }
            else if (control is PasswordBox passwordBox)
            {
                passwordBox.IsEnabled = !isReadOnly;
            }
            else if (control is Button button)
            {
                button.IsEnabled = !isReadOnly;
            }
            else
            {
                // Generic fallback for FrameworkElement
                control.IsEnabled = !isReadOnly;
            }
        }

        // Update Visibility state for controls with dynamic visibility
        foreach (var (control, isVisibleGetter) in _visibilityControls)
        {
            control.Visibility = isVisibleGetter() ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void OnReadOnlyControlCreated(FrameworkElement control, Func<bool> isReadOnlyGetter)
    {
        _readOnlyControls.Add((control, isReadOnlyGetter));
    }

    private void OnVisibilityControlCreated(FrameworkElement control, Func<bool> isVisibleGetter)
    {
        _visibilityControls.Add((control, isVisibleGetter));
    }

    private void OnCultureChanged(object? sender, CultureChangedEventArgs e)
    {
        UpdateLocalizedTexts();
    }

    private void OnLocalizationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Item[]")
        {
            UpdateLocalizedTexts();
        }
    }

    private void UpdateLocalizedTexts()
    {
        var service = _localizationService;
        if (service == null) return;

        foreach (var (textBlock, resourceKey, fallbackText) in _localizedTextBlocks)
        {
            if (!string.IsNullOrEmpty(resourceKey))
            {
                textBlock.Text = service.GetString(resourceKey);
            }
        }

        foreach (var (control, placeholderKey, placeholderFallback) in _localizedPlaceholders)
        {
            if (!string.IsNullOrEmpty(placeholderKey))
            {
                var localizedPlaceholder = service.GetString(placeholderKey);
                if (control is TextBox textBox)
                {
                    textBox.Tag = localizedPlaceholder;
                }
            }
        }

        foreach (var navNode in _navigationNodes)
        {
            UpdateNavigationNodeTitle(navNode, service);
        }
    }

    private void UpdateNavigationNodeTitle(NavigationNode node, ICultureService service)
    {
        if (!string.IsNullOrEmpty(node.TitleKey))
        {
            node.Title = service.GetString(node.TitleKey);
        }

        foreach (var child in node.Children)
        {
            UpdateNavigationNodeTitle(child, service);
        }
    }

    private void SubscribeToPropertyChangedEvents(object target)
    {
        if (target is INotifyPropertyChanged notifyPropertyChanged && !_subscribedTargets.Contains(notifyPropertyChanged))
        {
            notifyPropertyChanged.PropertyChanged += OnTargetPropertyChanged;
            _subscribedTargets.Add(notifyPropertyChanged);
        }
    }

    private void UnsubscribeFromPropertyChangedEvents()
    {
        foreach (var target in _subscribedTargets)
        {
            target.PropertyChanged -= OnTargetPropertyChanged;
        }
        _subscribedTargets.Clear();
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Refresh commands and ReadOnly states when any property changes
        // This ensures UI updates when properties like Email or IsAdmin change
        Refresh();
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
        
        // O(1) lookup using the dictionary
        if (_sectionControlMap.TryGetValue(sectionId, out var element))
        {
            element.BringIntoView();
        }
    }

    private void BuildUI()
    {
        if (_formScrollViewer is null || Targets is null || _effectiveProvider is null) return;

        // Clear previous data
        _formSections.Clear();
        _sectionControlMap.Clear();
        _readOnlyControls.Clear();
        _visibilityControls.Clear();
        _localizedTextBlocks.Clear();
        _localizedPlaceholders.Clear();
        
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
            var factory = new WpfControlFactory(descriptor, PropertyAccessor);
            factory.ReadOnlyControlCreated += OnReadOnlyControlCreated;
            factory.VisibilityControlCreated += OnVisibilityControlCreated;

            var classSectionId = $"class_{classIndex}";
            
            // Create navigation node for this class
            var headerTitle = descriptor.MainHeader ?? descriptor.DisplayName;
            var headerTitleKey = descriptor.UseMainHeaderKey ? descriptor.MainHeader : null;
            
            if (!string.IsNullOrEmpty(headerTitleKey) && _localizationService != null)
            {
                var localized = _localizationService.GetString(headerTitleKey);
                if (!string.IsNullOrEmpty(localized))
                {
                    headerTitle = localized;
                }
            }
            
            var navNode = new NavigationNode(
                headerTitle,
                null, // icon
                classSectionId,
                descriptor,
                target,
                headerTitleKey);
            _navigationNodes.Add(navNode);

            // Create class-level form section
            var classHeader = CreateSectionHeader(headerTitle, classSectionId, true, descriptor.UseMainHeaderKey ? descriptor.MainHeader : null);
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
                var subTitleKey = subSection.UseTitleKey ? subSection.Title : null;
                var subTitle = subSection.Title;
                
                if (!string.IsNullOrEmpty(subTitleKey) && _localizationService != null)
                {
                    var localized = _localizationService.GetString(subTitleKey);
                    if (!string.IsNullOrEmpty(localized))
                    {
                        subTitle = localized;
                    }
                }
                
                // Create navigation child node for subsection
                var subNavNode = new NavigationNode(
                    subTitle,
                    subIndex,
                    subSectionId,
                    subSection,
                    navNode,
                    subTitleKey);
                navNode.Children.Add(subNavNode);

                // Create subsection header
                var subHeader = CreateSectionHeader(subSection.Title, subSectionId, false, subSection.UseTitleKey ? subSection.Title : null);
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

    private FrameworkElement CreateSectionHeader(string title, string sectionId, bool isMainHeader, string? titleKey = null)
    {
        var displayText = title;
        
        if (!string.IsNullOrEmpty(titleKey) && _localizationService != null)
        {
            var localized = _localizationService.GetString(titleKey);
            if (!string.IsNullOrEmpty(localized))
            {
                displayText = localized;
            }
        }
        
        var textBlock = new TextBlock 
        { 
            Text = displayText, 
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
        
        if (!string.IsNullOrEmpty(titleKey))
        {
            _localizedTextBlocks.Add((textBlock, titleKey, title));
        }
        
        _sectionControlMap[sectionId] = textBlock;
        
        return textBlock;
    }

    private UIElement CreatePropertyControl(WpfControlFactory factory, PropertyDescriptor prop, object target)
    {
        var panel = new Grid { Margin = new Thickness(0, 5, 0, 5) };
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Label
        var labelText = prop.DisplayName;
        if (prop.UseDisplayNameKey && !string.IsNullOrEmpty(prop.DisplayNameKey) && _localizationService != null)
        {
            var localized = _localizationService.GetString(prop.DisplayNameKey);
            if (!string.IsNullOrEmpty(localized))
            {
                labelText = localized;
            }
        }
        
        var label = new TextBlock 
        { 
            Text = labelText, 
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        
        if (prop.UseDisplayNameKey)
        {
            _localizedTextBlocks.Add((label, prop.DisplayNameKey, prop.DisplayName));
        }
        
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
