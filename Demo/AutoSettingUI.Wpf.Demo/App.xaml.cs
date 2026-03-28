using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DynamicLocalization.Core;
using DynamicLocalization.Core.Providers;
using AutoSettingUI.Wpf.Demo.Resources;

namespace AutoSettingUI.Wpf.Demo;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public new static App Current => (App)Application.Current!;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        Services = ConfigureServices();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ICultureService>(sp =>
        {
            var cultureService = new CultureService();
            var resxProvider = new ResxLocalizationProvider();
            resxProvider.Initialize(new ResxLocalizationProviderOptions
            {
                ResourceType = typeof(Strings)
            });
            cultureService.RegisterProvider(resxProvider);
            return cultureService;
        });

        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
