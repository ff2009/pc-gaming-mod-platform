using Avalonia.Platform.Storage;
using PCGamingModApp.Core.ViewModels;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDialogService
{
    Task ShowDialog<THost, TDialogViewModel>(THost host, TDialogViewModel dialogViewModel) 
        where THost : IDialogProvider
        where TDialogViewModel : DialogViewModel;
    
    Task<string?> FilePickerAsync(FilePickerOpenOptions? options = null);
    
    Task<string?> FolderPickerAsync(FolderPickerOpenOptions? options = null, string? suggestedStartLocation = null);
}