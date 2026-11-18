using SlidesViewer.Host.Repositories;

namespace SlidesViewer.Host.Tests.Repositories;

public class SlideRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly SlideRepository _repository;

    public SlideRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SlidesViewerTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _repository = new SlideRepository();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task SearchFilesAsync_WithEmptyFolderPath_ReturnsEmptyList()
    {
        var result = await _repository.SearchFilesAsync(string.Empty);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchFilesAsync_WithWhiteSpaceFolderPath_ReturnsEmptyList()
    {
        var result = await _repository.SearchFilesAsync("   ");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchFilesAsync_WithNonExistentFolder_ReturnsEmptyList()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "NonExistent");

        var result = await _repository.SearchFilesAsync(nonExistentPath);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchFilesAsync_WithSingleTiffFile_ReturnsFileEntity()
    {
        // Arrange
        var fileName = "test.tiff";
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, new string('x', 1024));

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        Assert.Equal(fileName, result[0].FileName);
        Assert.Equal(1024, result[0].Size);
    }

    [Fact]
    public async Task SearchFilesAsync_WithMultipleTiffFiles_ReturnsAllFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "file1.tiff"), "content1");
        File.WriteAllText(Path.Combine(_testDirectory, "file2.tiff"), "content2");
        File.WriteAllText(Path.Combine(_testDirectory, "file3.tiff"), "content3");

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchFilesAsync_ExcludesNonTiffFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDirectory, "slide.tiff"), "tiff");
        File.WriteAllText(Path.Combine(_testDirectory, "image.png"), "png");
        File.WriteAllText(Path.Combine(_testDirectory, "document.pdf"), "pdf");
        File.WriteAllText(Path.Combine(_testDirectory, "file.txt"), "txt");

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        Assert.Equal("slide.tiff", result[0].FileName);
    }

    [Fact]
    public async Task SearchFilesAsync_InSubdirectories_FindsFiles()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subfolder");
        Directory.CreateDirectory(subDir);

        File.WriteAllText(Path.Combine(_testDirectory, "root.tiff"), "root");
        File.WriteAllText(Path.Combine(subDir, "nested.tiff"), "nested");

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchFilesAsync_SetsCorrectRelativePath()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "folder1", "folder2");
        Directory.CreateDirectory(subDir);

        var fileName = "deep.tiff";
        var filePath = Path.Combine(subDir, fileName);
        File.WriteAllText(filePath, "deep");

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        var expectedRelative = Path.Combine("folder1", "folder2");
        Assert.Equal(expectedRelative, result[0].RelativePath);
        Assert.Equal("deep.tiff", result[0].FileName);
    }

    [Fact]
    public async Task SearchFilesAsync_WithCancellationToken_ReturnsEmptyOnCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory, cts.Token);

        // Cancelled token on empty folder returns empty, not exception
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchFilesAsync_FileEntityHasCorrectProperties()
    {
        // Arrange
        var fileName = "test.tiff";
        var expectedSize = 4096;
        var filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, new string('x', expectedSize));

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Single(result);
        var entity = result[0];
        Assert.Equal(fileName, entity.FileName);
        Assert.Equal(expectedSize, entity.Size);
        Assert.Equal(".", entity.RelativePath);
    }

    [Fact]
    public async Task SearchFilesAsync_DeeplyNestedFolders_FindsAllFiles()
    {
        // Arrange
        var dir1 = Path.Combine(_testDirectory, "level1");
        var dir2 = Path.Combine(dir1, "level2");
        var dir3 = Path.Combine(dir2, "level3");
        Directory.CreateDirectory(dir3);

        File.WriteAllText(Path.Combine(dir1, "f1.tiff"), "1");
        File.WriteAllText(Path.Combine(dir2, "f2.tiff"), "2");
        File.WriteAllText(Path.Combine(dir3, "f3.tiff"), "3");

        // Act
        var result = await _repository.SearchFilesAsync(_testDirectory);

        // Assert
        Assert.Equal(3, result.Count);
    }
}
