using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using PCGamingModApp.Core.Services.Implementations;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Tests.Unit.Services.Implementations;

public class GameManagerTests
{
    [Fact]
    public async Task SelectGameExecutable_ShouldReturnSelectedFilePath()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var gameRepository = new Mock<IGameRepository>();
        var gameIconService = new Mock<IGameIconService>();
        // Mock the file picker to return a predefined path
        dialogService
            .Setup(x => x.FilePickerAsync(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync("/path/to/game.exe");

        var service = new GameManager(dialogService.Object, gameRepository.Object, gameIconService.Object);

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
        var gameRepository = new Mock<IGameRepository>();
        var gameIconService = new Mock<IGameIconService>();
        // Mock the file picker to return null (user canceled)
        dialogService
            .Setup(x => x.FilePickerAsync(It.IsAny<FilePickerOpenOptions>()))
            .ReturnsAsync((string?)null);

        var service = new GameManager(dialogService.Object, gameRepository.Object, gameIconService.Object);

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

        var mockGameRepository = new Mock<IGameRepository>();
        var mockGameManagerService = new Mock<GameManager>();
        var mockMessenger = new Mock<IMessenger>();

        /*var viewModel = new GameMenuViewModel(mockGameRepository.Object, mockGameManagerService.Object, mockMessenger.Object);
        viewModel.AddNewGameCommand();*/

        Assert.True(File.Exists("/tmp/test_icons/TestGame.png"));
    }

    [Fact]
    public async Task AddGame_ShouldThrow_IfGameExists()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var gameRepository = new Mock<IGameRepository>();
        var gameIconService = new Mock<IGameIconService>();
        gameRepository.Setup(x => x.GetGameByInstallPath(It.IsAny<string>()))
            .ReturnsAsync(new GameDataModel());

        var service = new GameManager(dialogService.Object,
            gameRepository.Object,
            gameIconService.Object
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.AddGame("fake/path"));
    }

    [Fact]
    public async Task SaveIconAsync_ShouldReturnFilename_IfExtractionSucceeds()
    {
        // Arrange
        var mockAppPaths = new Mock<IAppPaths>();
        mockAppPaths.Setup(x => x.GameIcons).Returns("/fake/icons");
        var mockExtractor = new Mock<IIconExtractor>();
        mockExtractor.Setup(x => x.ExtractAndSaveAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new GameIconService(mockAppPaths.Object, mockExtractor.Object);

        // Act
        string? filename = await service.SaveIconAsync("MyGame", "fake/icon.ico");

        // Assert
        Assert.NotNull(filename);
        Assert.StartsWith("mygame-", Path.GetFileName(filename));
    }
}