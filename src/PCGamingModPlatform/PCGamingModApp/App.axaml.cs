using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.MainApp;
using PCGamingModApp.ViewModels;
using PCGamingModApp.Views;
using System;
using System.Linq;

namespace PCGamingModApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<MainViewModel>();

        collection.AddSingleton<MenuViewModel>();

        collection.AddSingleton<HomePageViewModel>();

        collection.AddTransient<BasePageViewModel>();
        collection.AddTransient<GameSettingsPageViewModel>();

        collection.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch {
            _ when type == typeof(BasePageViewModel) => x.GetRequiredService<BasePageViewModel>(),
            _ when type == typeof(GameSettingsPageViewModel) => x.GetRequiredService<GameSettingsPageViewModel>(),
            _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
        });

        collection.AddSingleton<PageFactory>();

        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainViewModel>()
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