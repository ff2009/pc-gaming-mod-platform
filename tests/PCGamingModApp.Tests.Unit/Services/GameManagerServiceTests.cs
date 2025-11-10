using Avalonia.Platform.Storage;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Tests.Unit.Services;

public class GameManagerServiceTests
{
    [Fact]
    public async Task SelectGameExecutable_ShouldReturnSelectedFilePath()
    {
        // Arrange
        var appPaths = new Mock<IAppPaths>();
        var dialogService = new Mock<IDialogService>();
        var gameRepository = new Mock<IGameRepository>();
        // Mock the file picker to return a predefined path
        dialogService
            .Setup(x => x.FilePickerAsync(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync("/path/to/game.exe");

        var service = new GameManagerService(appPaths.Object, dialogService.Object, gameRepository.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Equal("/path/to/game.exe", result);
    }

    [Fact]
    public async Task SelectGameExecutable_ShouldReturnNull_WhenUserCancels()
    {
        // Arrange
        var appPaths = new Mock<IAppPaths>();
        var dialogService = new Mock<IDialogService>();
        var gameRepository = new Mock<IGameRepository>();
        // Mock the file picker to return null (user canceled)
        dialogService
            .Setup(x => x.FilePickerAsync(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync((string?)null);

        var service = new GameManagerService(appPaths.Object, dialogService.Object, gameRepository.Object);

        // Act
        var result = await service.SelectGameExecutableAsync();

        // Assert
        Assert.Null(result);
    }
    
    [Fact(Skip = "File system dependent test")]
    public void AddGameIcon_SavesToCorrectPath()
    {
        var mockAppPaths = new Mock<IAppPaths>();
        mockAppPaths.SetupGet(x => x.GameIcons).Returns("/tmp/test_icons");
        
        /*var mockGameRepository = new Mock<IGameRepository>();
        var mockMessenger = new Mock<IMessenger>();

        var viewModel = new GameMenuViewModel(mockGameRepository.Object, mockMessenger.Object, null);
        viewModel.AddGameIcon(new Icon("dummy.ico"), "TestGame");*/

        Assert.True(File.Exists("/tmp/test_icons/TestGame.png"));
    }
}