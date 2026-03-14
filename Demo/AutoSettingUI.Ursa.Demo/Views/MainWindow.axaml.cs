using Avalonia.Controls;

namespace AutoSettingUI.Ursa.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Explicitly set the provider for AOT compatibility.
        // This ensures the GeneratedSettingProvider type is referenced and not trimmed.
        try 
        {
            var provider = new AutoSettingUI.Generated.GeneratedSettingProvider();
            var defaultPanel = this.FindControl<AutoSettingUI.Ursa.Controls.UrsaAutoSettingPanel>("DefaultSettingsPanel");
            if (defaultPanel != null)
            {
                defaultPanel.DescriptorProvider = provider;
                defaultPanel.PropertyAccessor = provider;
            }
            
            var customPanel = this.FindControl<AutoSettingUI.Ursa.Controls.UrsaAutoSettingPanel>("CustomSettingsPanel");
            if (customPanel != null)
            {
                customPanel.DescriptorProvider = provider;
                customPanel.PropertyAccessor = provider;
            }
        }
        catch { }
    }
}