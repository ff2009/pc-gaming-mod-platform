using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Core.ViewModels;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class DialogService(Func<TopLevel?> topLevel) : IDialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host, TDialogViewModel dialogViewModel)
        where THost : IDialogProvider
        where TDialogViewModel : DialogViewModel
    {
        // Set host dialog to provide one
        host.Dialog = dialogViewModel;
        dialogViewModel.Show();

        // Wait for dialog to close
        await dialogViewModel.WaitAsync();
    }

    public async Task<string?> FilePickerAsync(FilePickerOpenOptions? options = null)
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

    public async Task<string?> FolderPickerAsync(FolderPickerOpenOptions? options = null)
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