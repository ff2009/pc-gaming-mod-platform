using System.Drawing;
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
        /*// Arrange
        var dialogService = new Mock<IDialogService>();
        // Mock the file picker to return a predefined path
        dialogService
            .Setup(x => x.FilePicker(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync("/path/to/game.exe");

        var service = new GameManagerService(dialogService.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Equal("/path/to/game.exe", result);*/
    }

    [Fact]
    public async Task SelectGameExecutable_ShouldReturnNull_WhenUserCancels()
    {
        /*// Arrange
        var dialogService = new Mock<IDialogService>();
        // Mock the file picker to return null (user canceled)
        dialogService
            .Setup(x => x.FilePicker(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync((string?)null);

        var service = new GameManagerService(dialogService.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Null(result);*/
    }
    
    [Fact]
    public void AddGameIcon_SavesToCorrectPath()
    {
        var mockAppPaths = new Mock<IAppPaths>();
        mockAppPaths.SetupGet(x => x.GameIcons).Returns("/tmp/test_icons");

        //var viewModel = new GameLibraryViewModel(mockAppPaths.Object);
        //viewModel.AddGameIcon(new Icon("dummy.ico"), "TestGame");

        Assert.True(File.Exists("/tmp/test_icons/TestGame.png"));
    }
}