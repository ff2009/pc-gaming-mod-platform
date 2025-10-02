using Avalonia.Platform.Storage;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDialogService
{
    Task<string?> FilePicker(FilePickerOpenOptions? options = null);
    
    Task<string?> FolderPicker(FolderPickerOpenOptions? options = null);
}