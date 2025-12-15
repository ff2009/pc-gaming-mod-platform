using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PCGamingModApp.Core.MainApp;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Core.ViewModels;

namespace PCGamingModApp.Core.Dependencies;

public static class ServiceCollectionExtensions
{
    extension (IServiceCollection services)
    {
        public void AddCoreServices()
        {        
            // Step 1: Create and initialize AppPaths
            IAppPaths appPaths = new AppPaths();
            appPaths.EnsureCreated();
            appPaths.Migrate();
        
            services.AddHttpClient();
            services.AddSingleton(appPaths);
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IImageCache, SimpleImageCache>(x=>
                new SimpleImageCache(Path.Combine(appPaths.GameIcons)));
        
            // Register GameInstallationService
            services.AddTransient<IGameManager, GameManager>();

            services.AddSingleton<IIconExtractor, IconExtractorService>();
            services.AddSingleton<IGameIconService, GameIconService>();
            services.AddTransient<ILauncherService, LauncherService>();
            
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        }
        
        public void AddDownloadServices()
        {
            services.AddSingleton<IDownloadManager, DownloadManager>();
            services.AddTransient<IDownloadService, DownloadService>();
        }
        
        public void AddViewModels()
        {
            services.AddSingleton<MainViewModel>();
        
            // UI ViewModels (transient – new instance per view)
            services.AddSingleton<MenuViewModel>();
            services.AddSingleton<GameMenuViewModel>();
            services.AddTransient<GameItemViewModel>(); // each row gets its own VM

            services.AddSingleton<HomePageViewModel>();

            services.AddTransient<BasePageViewModel>();
            services.AddTransient<GameSettingsPageViewModel>();
            services.AddTransient<DownloadsPageViewModel>();
            services.AddTransient<DownloadItemViewModel>(); // each row gets its own VM
            services.AddScoped<NewDownloadDialogViewModel>();
            services.AddTransient<AddOnsPageViewModel>();
            services.AddTransient<SystemPageViewModel>();
            services.AddTransient<AboutPageViewModel>();

            services.AddTransient<DeveloperSettingsPageViewModel>();
            services.AddScoped<ConfirmDialogViewModel>();

            services.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
            {
                _ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
                _ when type == typeof(BasePageViewModel) => x.GetRequiredService<BasePageViewModel>(),
                _ when type == typeof(GameSettingsPageViewModel) => x.GetRequiredService<GameSettingsPageViewModel>(),
                _ when type == typeof(DownloadsPageViewModel) => x.GetRequiredService<DownloadsPageViewModel>(),
                _ when type == typeof(AddOnsPageViewModel) => x.GetRequiredService<AddOnsPageViewModel>(),
                _ when type == typeof(SystemPageViewModel) => x.GetRequiredService<SystemPageViewModel>(),
                _ when type == typeof(AboutPageViewModel) => x.GetRequiredService<AboutPageViewModel>(),
                _ when type == typeof(DeveloperSettingsPageViewModel) => x
                    .GetRequiredService<DeveloperSettingsPageViewModel>(),
                _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
            });

            services.AddSingleton<PageFactory>();
        }
    }
}