using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Data.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Core.ViewModels;
using PCGamingModApp.Views;
using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using PCGamingModApp.Core.Dependencies;

namespace PCGamingModApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        services.AddDownloadServices();
        services.AddViewModels();

        var serviceProvider = services.BuildServiceProvider();
        var appPaths = serviceProvider.GetRequiredService<IAppPaths>();
        services.AddDataRepository(connectionString: $"DataSource={Path.Combine(appPaths.Database, "pcgamingmod.db")}");

        // TopLevel provider
        services.AddSingleton<Func<TopLevel?>>(x => () =>
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime topWindow)
                return TopLevel.GetTopLevel(topWindow.MainWindow);
            if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                return TopLevel.GetTopLevel(singleViewPlatform.MainView);

            return null;
        });

        serviceProvider = services.BuildServiceProvider();
        serviceProvider.InitializeDatabase();
        serviceProvider.InitializeDownloadManager();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}