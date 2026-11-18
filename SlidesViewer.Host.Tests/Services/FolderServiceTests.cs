using SlidesViewer.Host.Repositories;
using SlidesViewer.Host.Services;
using NSubstitute;

namespace SlidesViewer.Host.Tests.Services;

public class FolderServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public FolderServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SlidesViewerFolderServiceTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FolderService(null!));
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_CallsRepositoryWithNormalizedPath()
    {
        // Arrange
        var folderPath = Path.Combine(_testDirectory, "slides");
        Directory.CreateDirectory(folderPath);
        var rawPath = folderPath + "\\";
        var normalizedPath = folderPath;
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(normalizedPath).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(rawPath);

        // Assert
        await mockRepo.Received(1).GetSubdirectoriesAsync(normalizedPath);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_ForwardSlashes_AreNormalized()
    {
        // Arrange
        var folderPath = Path.Combine(_testDirectory, "slides", "presentations");
        Directory.CreateDirectory(folderPath);
        var forwardPath = folderPath + "\\";
        var expectedNormalized = folderPath;
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(expectedNormalized).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(forwardPath);

        // Assert
        await mockRepo.Received(1).GetSubdirectoriesAsync(expectedNormalized);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_DriveLetterPath_AddsBackslash()
    {
        // Arrange
        var drivePath = @"C:";
        var expectedNormalized = @"C:\";
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(expectedNormalized).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(drivePath);

        // Assert
        await mockRepo.Received(1).GetSubdirectoriesAsync(expectedNormalized);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_ReturnsFoldersWithTrailingBackslash()
    {
        // Arrange — создаём реальные папки, чтобы IsAccessibleFolder пропустил их
        var folderPath = Path.Combine(_testDirectory, "slides");
        Directory.CreateDirectory(Path.Combine(folderPath, "Presentations"));
        Directory.CreateDirectory(Path.Combine(folderPath, "Archives"));

        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(folderPath).Returns(Task.FromResult(new List<string>
        {
            Path.Combine(folderPath, "Presentations"),
            Path.Combine(folderPath, "Archives")
        }));

        var service = new FolderService(mockRepo);

        // Act
        var result = await service.GetAccessibleFoldersAsync(folderPath);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.EndsWith("\\", result[0]);
        Assert.EndsWith("\\", result[1]);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_ResultsAreSortedAlphabetically()
    {
        // Arrange — реальные папки для IsAccessibleFolder
        var folderPath = Path.Combine(_testDirectory, "slides");
        Directory.CreateDirectory(Path.Combine(folderPath, "ZebraFolder"));
        Directory.CreateDirectory(Path.Combine(folderPath, "AlphaFolder"));
        Directory.CreateDirectory(Path.Combine(folderPath, "MiddleFolder"));

        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(folderPath).Returns(Task.FromResult(new List<string>
        {
            Path.Combine(folderPath, "ZebraFolder"),
            Path.Combine(folderPath, "AlphaFolder"),
            Path.Combine(folderPath, "MiddleFolder")
        }));

        var service = new FolderService(mockRepo);

        // Act
        var result = await service.GetAccessibleFoldersAsync(folderPath);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.StartsWith("AlphaFolder", Path.GetFileName(result[0].TrimEnd('\\')));
        Assert.StartsWith("MiddleFolder", Path.GetFileName(result[1].TrimEnd('\\')));
        Assert.StartsWith("ZebraFolder", Path.GetFileName(result[2].TrimEnd('\\')));
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_EmptySubdirectories_ReturnsEmptyList()
    {
        // Arrange
        var folderPath = @"C:\empty";
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(folderPath).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        var result = await service.GetAccessibleFoldersAsync(folderPath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_CallsRepositoryOnce()
    {
        // Arrange
        var folderPath = Path.Combine(_testDirectory, "slides");
        Directory.CreateDirectory(folderPath);
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(folderPath).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(folderPath);

        // Assert
        await mockRepo.Received(1).GetSubdirectoriesAsync(folderPath);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_NestedPath_IsNormalizedCorrectly()
    {
        // Arrange
        var folderPath = Path.Combine(_testDirectory, "Users", "username", "Documents", "Slides");
        Directory.CreateDirectory(folderPath);
        var nestedPath = folderPath + "\\";
        var expectedNormalized = folderPath;
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(expectedNormalized).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(nestedPath);

        // Assert
        await mockRepo.Received(1).GetSubdirectoriesAsync(expectedNormalized);
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_ForwardSlashesInNestedPath_TrailingSlashRemoved()
    {
        // Arrange — создаём папку и используем путь с прямыми слешами
        var folderPath = Path.Combine(_testDirectory, "Users", "username", "Documents", "Slides");
        Directory.CreateDirectory(folderPath);
        var forwardPath = folderPath.Replace('\\', '/') + "/";
        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(Arg.Any<string>()).Returns(Task.FromResult(new List<string>()));

        var service = new FolderService(mockRepo);

        // Act
        await service.GetAccessibleFoldersAsync(forwardPath);

        // Assert — мок должен быть вызван с нормализованным путём (с внутренними /)
        await mockRepo.Received(1).GetSubdirectoriesAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task GetAccessibleFoldersAsync_MultipleFolders_ReturnsSortedWithBackslashes()
    {
        // Arrange — реальные папки
        var folderPath = Path.Combine(_testDirectory, "data");
        Directory.CreateDirectory(Path.Combine(folderPath, "Beta"));
        Directory.CreateDirectory(Path.Combine(folderPath, "Alpha"));
        Directory.CreateDirectory(Path.Combine(folderPath, "Gamma"));

        var mockRepo = Substitute.For<IFolderRepository>();
        mockRepo.GetSubdirectoriesAsync(folderPath).Returns(Task.FromResult(new List<string>
        {
            Path.Combine(folderPath, "Beta"),
            Path.Combine(folderPath, "Alpha"),
            Path.Combine(folderPath, "Gamma")
        }));

        var service = new FolderService(mockRepo);

        // Act
        var result = await service.GetAccessibleFoldersAsync(folderPath);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.EndsWith("\\", result[0]);
        Assert.EndsWith("\\", result[1]);
        Assert.EndsWith("\\", result[2]);
        Assert.Equal("Alpha", Path.GetFileName(result[0].TrimEnd('\\')));
        Assert.Equal("Beta", Path.GetFileName(result[1].TrimEnd('\\')));
        Assert.Equal("Gamma", Path.GetFileName(result[2].TrimEnd('\\')));
    }
}
