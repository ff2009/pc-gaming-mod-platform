using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.Services.Implementations;

public class DialogService(Func<TopLevel?> topLevel) : IDialogService
{
    public async Task<string?> FilePicker(FilePickerOpenOptions? options = null)
    {
        TopLevel? topLevelVisual = topLevel();
        if (topLevelVisual == null) return null;

        options ??= new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select a file"
        };

        IReadOnlyList<IStorageFile> files = await topLevelVisual.StorageProvider.OpenFilePickerAsync(options);

        Uri? path = files.FirstOrDefault()?.Path;
        if (path == null) return null;
        return path.IsAbsoluteUri ? path.LocalPath : path.OriginalString;
    }

    public async Task<string?> FolderPicker(FolderPickerOpenOptions? options = null)
    {
        TopLevel? topLevelVisual = topLevel();
        if (topLevelVisual == null) return null;

        options ??= new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Select a folder"
        };

        IReadOnlyList<IStorageFolder> folders = await topLevelVisual.StorageProvider.OpenFolderPickerAsync(options);

        Uri? path = folders.FirstOrDefault()?.Path;
        if (path == null) return null;
        return path.IsAbsoluteUri ? path.LocalPath : path.OriginalString;
    }
}