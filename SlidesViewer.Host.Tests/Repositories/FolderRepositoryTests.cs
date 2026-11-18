using SlidesViewer.Host.Repositories;

namespace SlidesViewer.Host.Tests.Repositories;

public class FolderRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FolderRepository _repository;

    public FolderRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SlidesViewerFolderRepoTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _repository = new FolderRepository();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithEmptyPath_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.GetSubdirectoriesAsync(string.Empty));

        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithNonExistentPath_ThrowsDirectoryNotFoundException()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            () => _repository.GetSubdirectoriesAsync(nonExistentPath));
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithSingleSubdirectory_ReturnsIt()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subfolder1");
        Directory.CreateDirectory(subDir);

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        Assert.Contains(subDir, result);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithMultipleSubdirectories_ReturnsAll()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectory, "alpha"));
        Directory.CreateDirectory(Path.Combine(_testDirectory, "beta"));
        Directory.CreateDirectory(Path.Combine(_testDirectory, "gamma"));

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_ReturnsFullPaths()
    {
        // Arrange
        var subDirName = "test_folder";
        var expectedFullPath = Path.Combine(_testDirectory, subDirName);
        Directory.CreateDirectory(expectedFullPath);

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedFullPath, result[0]);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_TopDirectoryOnly_DoesNotReturnNested()
    {
        // Arrange
        var topDir = Path.Combine(_testDirectory, "top");
        Directory.CreateDirectory(topDir);
        var nestedDir = Path.Combine(topDir, "nested");
        Directory.CreateDirectory(nestedDir);

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert — только top, не nested
        Assert.Single(result);
        Assert.Equal(topDir, result[0]);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithSpecialCharactersInName_ReturnsCorrectly()
    {
        // Arrange
        var specialDir = Path.Combine(_testDirectory, "folder_with-dash");
        Directory.CreateDirectory(specialDir);
        var spaceDir = Path.Combine(_testDirectory, "folder with space");
        Directory.CreateDirectory(spaceDir);

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetSubdirectoriesAsync_WithCancellationToken_ReturnsResult()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_testDirectory, "dir1"));

        // Act
        var result = await _repository.GetSubdirectoriesAsync(_testDirectory);

        // Assert
        Assert.NotNull(result);
    }
}
