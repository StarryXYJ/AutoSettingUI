using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AutoSettingUI.Ursa.Demo.ViewModels;
using AutoSettingUI.Ursa.Demo.Views;
using Microsoft.Extensions.DependencyInjection;
using DynamicLocalization.Core;
using DynamicLocalization.Core.Providers;
using AutoSettingUI.Ursa.Demo.Resources;

namespace AutoSettingUI.Ursa.Demo;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public new static App Current => (App)Application.Current!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            Services = ConfigureServices();

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
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

        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
