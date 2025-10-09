using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
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
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using PCGamingModApp.Data;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Step 1: Create and initialize AppPaths
        AppPaths appPaths = new AppPaths();
        appPaths.EnsureCreated();
        appPaths.Migrate();
        
        var services = new ServiceCollection();

        services.AddSingleton<IAppPaths>(appPaths);
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IImageCache, SimpleImageCache>();
        
        // Register GameInstallationService
        services.AddTransient<GameManagerService>();

        services.AddTransient<ILauncherService, LauncherService>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<MainViewModel>();

        // UI ViewModels (transient – new instance per view)
        services.AddSingleton<MenuViewModel>();
        services.AddSingleton<GameMenuViewModel>();
        services.AddTransient<GameItemViewModel>(); // each row gets its own VM
        
        services.AddTransient<IGameRepository, GameRepository>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.Database}"));
        
        services.AddSingleton<HomePageViewModel>();

        services.AddTransient<BasePageViewModel>();
        services.AddTransient<GameSettingsPageViewModel>();
        services.AddTransient<AddOnsPageViewModel>();
        services.AddTransient<SystemPageViewModel>();
        services.AddTransient<AboutPageViewModel>();

        services.AddTransient<DeveloperSettingsPageViewModel>();

        services.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
        {
            _ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
            _ when type == typeof(BasePageViewModel) => x.GetRequiredService<BasePageViewModel>(),
            _ when type == typeof(GameSettingsPageViewModel) => x.GetRequiredService<GameSettingsPageViewModel>(),
            _ when type == typeof(AddOnsPageViewModel) => x.GetRequiredService<AddOnsPageViewModel>(),
            _ when type == typeof(SystemPageViewModel) => x.GetRequiredService<SystemPageViewModel>(),
            _ when type == typeof(AboutPageViewModel) => x.GetRequiredService<AboutPageViewModel>(),
            _ when type == typeof(DeveloperSettingsPageViewModel) => x
                .GetRequiredService<DeveloperSettingsPageViewModel>(),
            _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
        });

        services.AddSingleton<PageFactory>();
        
        // TopLevel provider
        services.AddSingleton<Func<TopLevel?>>(x => () => {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime topWindow)
                return TopLevel.GetTopLevel(topWindow.MainWindow);
            if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                return TopLevel.GetTopLevel(singleViewPlatform.MainView);

            return null;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Initialize AppPaths
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        // Initialize and migrate the database
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();

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