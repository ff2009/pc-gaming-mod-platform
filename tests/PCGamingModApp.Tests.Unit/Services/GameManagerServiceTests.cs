using Avalonia.Platform.Storage;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Tests.Unit.Services;

public class GameManagerServiceTests
{
    [Fact]
    public async Task SelectGameExecutable_ShouldReturnSelectedFilePath()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        // Mock the file picker to return a predefined path
        dialogService
            .Setup(x => x.FilePicker(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync("/path/to/game.exe");

        var service = new GameManagerService(dialogService.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Equal("/path/to/game.exe", result);
    }

    [Fact]
    public async Task SelectGameExecutable_ShouldReturnNull_WhenUserCancels()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        // Mock the file picker to return null (user canceled)
        dialogService
            .Setup(x => x.FilePicker(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync((string?)null);

        var service = new GameManagerService(dialogService.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Null(result);
    }
}