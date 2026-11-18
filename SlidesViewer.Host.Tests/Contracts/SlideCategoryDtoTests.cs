using SlidesViewer.Host.Contracts.Categories;

namespace SlidesViewer.Host.Tests.Contracts;

public class SlideCategoryDtoClassTests
{
    [Fact]
    public void Constructor_WithAllParameters_CreatesDto()
    {
        // Arrange
        var paths = new List<string> { "path1.tiff", "path2.tiff" };

        // Act
        var dto = new SlideCategoryDto("TestCategory", 5, paths);

        // Assert
        Assert.Equal("TestCategory", dto.Name);
        Assert.Equal(5, dto.FileCount);
        Assert.Equal(2, dto.RelativePaths.Count);
        Assert.Contains("path1.tiff", dto.RelativePaths);
        Assert.Contains("path2.tiff", dto.RelativePaths);
    }

    [Fact]
    public void Constructor_WithTwoParams_CreatesDtoWithEmptyPaths()
    {
        // Act
        var dto = new SlideCategoryDto("TestCategory", 3);

        // Assert
        Assert.Equal("TestCategory", dto.Name);
        Assert.Equal(3, dto.FileCount);
        Assert.Empty(dto.RelativePaths);
    }

    [Fact]
    public void Constructor_WithNullPaths_CreatesEmptyList()
    {
        // Constructor заменяет null на пустой список
        var dto = new SlideCategoryDto("Test", 1, null!);
        Assert.Empty(dto.RelativePaths);
    }

    [Fact]
    public void Properties_Are_Initialized_Correctly()
    {
        // Arrange
        var paths = new List<string> { "a.tiff" };
        var dto = new SlideCategoryDto("Cat", 2, paths);

        // Act & Assert
        Assert.Equal("Cat", dto.Name);
        Assert.Equal(2, dto.FileCount);
        Assert.Single(dto.RelativePaths);
    }

    [Fact]
    public void Clone_CreatesNewInstanceWithChangedName()
    {
        // Arrange
        var paths = new List<string> { "a.tiff" };
        var dto = new SlideCategoryDto("Cat", 2, paths);
        var cloned = new SlideCategoryDto("NewCat", dto.FileCount, dto.RelativePaths);

        // Assert
        Assert.Equal("NewCat", cloned.Name);
        Assert.Equal(2, cloned.FileCount);
        Assert.Single(cloned.RelativePaths);
        // Original unchanged
        Assert.Equal("Cat", dto.Name);
    }

    [Fact]
    public void Reference_Equality_SameInstanceAreEqual()
    {
        // Arrange
        var paths = new List<string> { "a.tiff" };
        var dto = new SlideCategoryDto("Cat", 1, paths);

        // Act & Assert
        Assert.Same(dto, dto);
    }

    [Fact]
    public void DifferentInstances_AreNotSameReference()
    {
        // Arrange
        var paths = new List<string> { "a.tiff" };
        var dto1 = new SlideCategoryDto("Cat1", 1, paths);
        var dto2 = new SlideCategoryDto("Cat2", 1, paths);

        // Act & Assert
        Assert.NotSame(dto1, dto2);
    }

    [Fact]
    public void ToString_ReturnsTypeName()
    {
        // Arrange
        var paths = new List<string> { "a.tiff", "b.tiff" };
        var dto = new SlideCategoryDto("Test", 2, paths);

        // Act
        var str = dto.ToString();

        // Assert
        Assert.NotNull(str);
        Assert.Contains("SlideCategoryDto", str);
    }
}
