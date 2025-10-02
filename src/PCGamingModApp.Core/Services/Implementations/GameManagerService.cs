using Avalonia.Platform.Storage;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.Services.Implementations;

public class GameManagerService(IDialogService dialogService)
{
    public async Task<string?> SelectGameExecutableAsync()
    {
        FilePickerOpenOptions options = new()
        {
            Title = "Select Game Executable",
            FileTypeFilter =
            [
                new FilePickerFileType("Executables")
                {
                    Patterns = ["*.exe", "*.bin", "*.app"]
                }
            ],
            AllowMultiple = false
        };
        
        string? result = await dialogService.FilePicker(options);
        
        
        
        return result;
    }
}