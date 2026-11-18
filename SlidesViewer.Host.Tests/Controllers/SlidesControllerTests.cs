using Microsoft.AspNetCore.Mvc;
using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Host.Services;
using NSubstitute;
using SlidesViewer.Host.Controllers;

namespace SlidesViewer.Host.Tests.Controllers;

public class SlidesControllerTests
{
    private readonly ISlideService _mockSlideService;
    private readonly IFolderService _mockFolderService;
    private readonly SlidesController _controller;

    public SlidesControllerTests()
    {
        _mockSlideService = Substitute.For<ISlideService>();
        _mockFolderService = Substitute.For<IFolderService>();
        _controller = new SlidesController(_mockSlideService, _mockFolderService);
    }

    #region Constructor null checks

    [Fact]
    public void Constructor_WithNullSlideService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlidesController(null!, Substitute.For<IFolderService>()));
    }

    [Fact]
    public void Constructor_WithNullFolderService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlidesController(Substitute.For<ISlideService>(), null!));
    }

    #endregion

    #region GetCategories

    [Fact]
    public async Task GetCategories_WithValidFolderPath_ReturnsOkResult()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var categories = new List<SlideCategoryDto>
        {
            new("CategoryA", 5),
            new("CategoryB", 3)
        };
        var response = new GetCategoriesResponse
        {
            Categories = categories,
            TotalFiles = 8,
            TotalCategories = 2,
            TotalUniquePaths = 6
        };

        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetCategories_CallsServiceWithCorrectFolderPath()
    {
        // Arrange
        var folderPath = @"C:\test\folder";
        var response = new GetCategoriesResponse { Categories = new List<SlideCategoryDto>() };
        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        await _controller.GetCategories(request);

        // Assert
        await _mockSlideService.Received(1).GetCategories(folderPath);
    }

    [Fact]
    public async Task GetCategories_WithEmptyFolderPath_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetCategoriesRequest { FolderPath = string.Empty };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCategories_WithWhiteSpaceFolderPath_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetCategoriesRequest { FolderPath = "   " };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCategories_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetCategories(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCategories_WithEmptyCategoriesList_ReturnsOkResultWithEmptyList()
    {
        // Arrange
        var folderPath = @"C:\empty";
        var response = new GetCategoriesResponse { Categories = new List<SlideCategoryDto>() };
        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        Assert.NotNull(okResult.Value);
        var returnedResponse = Assert.IsType<GetCategoriesResponse>(okResult.Value);
        Assert.Empty(returnedResponse.Categories);
    }

    [Fact]
    public async Task GetCategories_ReturnsCorrectActionResultType()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var categories = new List<SlideCategoryDto> { new("Test", 1) };
        var response = new GetCategoriesResponse { Categories = categories };
        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<GetCategoriesResponse>>(result);
    }

    [Fact]
    public async Task GetCategories_ServiceException_PropagatesException()
    {
        // Arrange
        var folderPath = @"C:\slides";
        _mockSlideService.GetCategories(folderPath).Returns(Task.FromException<GetCategoriesResponse>(
            new InvalidOperationException("Service error")));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetCategories(request));
        Assert.Equal("Service error", exception.Message);
    }

    [Fact]
    public async Task GetCategories_ReturnsCategoriesWithRelativePaths()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var categories = new List<SlideCategoryDto>
        {
            new("CategoryA", 2, new List<string> { "folder1/a1.tiff", "folder2/a2.tiff" }),
            new("CategoryB", 1, new List<string> { "folder3/b1.tiff" })
        };
        var response = new GetCategoriesResponse
        {
            Categories = categories,
            TotalFiles = 3,
            TotalCategories = 2,
            TotalUniquePaths = 3
        };

        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        var returnedResponse = Assert.IsType<GetCategoriesResponse>(okResult.Value);
        Assert.Equal(2, returnedResponse.Categories.Count);

        var catA = returnedResponse.Categories.First(c => c.Name == "CategoryA");
        Assert.Equal(2, catA.FileCount);
        Assert.Equal(2, catA.RelativePaths.Count);
        Assert.Contains("folder1/a1.tiff", catA.RelativePaths);
        Assert.Contains("folder2/a2.tiff", catA.RelativePaths);

        var catB = returnedResponse.Categories.First(c => c.Name == "CategoryB");
        Assert.Equal(1, catB.FileCount);
        Assert.Single(catB.RelativePaths);
        Assert.Equal("folder3/b1.tiff", catB.RelativePaths[0]);
    }

    [Fact]
    public async Task GetCategories_ReturnsCorrectCounters()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var categories = new List<SlideCategoryDto>
        {
            new("CategoryA", 5, new List<string> { "a1.tiff", "a2.tiff" }),
            new("CategoryB", 3, new List<string> { "b1.tiff" })
        };
        var response = new GetCategoriesResponse
        {
            Categories = categories,
            TotalFiles = 8,
            TotalCategories = 2,
            TotalUniquePaths = 3
        };

        _mockSlideService.GetCategories(folderPath).Returns(Task.FromResult(response));

        var request = new GetCategoriesRequest { FolderPath = folderPath };

        // Act
        var result = await _controller.GetCategories(request);

        // Assert
        var okResult = (OkObjectResult)result.Result;
        var returnedResponse = Assert.IsType<GetCategoriesResponse>(okResult.Value);
        Assert.Equal(8, returnedResponse.TotalFiles);
        Assert.Equal(2, returnedResponse.TotalCategories);
        Assert.Equal(3, returnedResponse.TotalUniquePaths);
    }

    #endregion
}
