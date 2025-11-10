using Avalonia.Platform.Storage;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDialogService
{
    Task<string?> FilePickerAsync(FilePickerOpenOptions? options = null);
    
    Task<string?> FolderPickerAsync(FolderPickerOpenOptions? options = null);
}