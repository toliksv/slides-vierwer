using Microsoft.AspNetCore.Mvc;
using SlidesViewer.Host.Services;
using NSubstitute;
using SlidesViewer.Host.Controllers;

namespace SlidesViewer.Host.Tests.Controllers;

public class SlidesControllerFoldersTests
{
    private readonly ISlideService _mockSlideService;
    private readonly IFolderService _mockFolderService;
    private readonly SlidesController _controller;

    public SlidesControllerFoldersTests()
    {
        _mockSlideService = Substitute.For<ISlideService>();
        _mockFolderService = Substitute.For<IFolderService>();
        _controller = new SlidesController(_mockSlideService, _mockFolderService);
    }

    #region GetFolders

    [Fact]
    public async Task GetFolders_WithValidPrefix_ReturnsOkResult()
    {
        // Arrange
        var prefix = @"C:\Slides\";
        var folders = new List<string>
        {
            @"C:\Slides\Presentations\",
            @"C:\Slides\Archives\"
        };

        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(folders));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        Assert.Equal(folders, okResult.Value);
    }

    [Fact]
    public async Task GetFolders_CallsServiceWithCorrectPrefix()
    {
        // Arrange
        var prefix = @"C:\Users\test\";
        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(new List<string>()));

        // Act
        await _controller.GetFolders(prefix);

        // Assert
        await _mockFolderService.Received(1).GetAccessibleFoldersAsync(prefix);
    }

    [Fact]
    public async Task GetFolders_WithEmptyPrefix_ReturnsBadRequest()
    {
        // Arrange
        var emptyPrefix = string.Empty;

        // Act
        var result = await _controller.GetFolders(emptyPrefix);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFolders_WithWhiteSpacePrefix_ReturnsBadRequest()
    {
        // Arrange
        var whiteSpacePrefix = "   ";

        // Act
        var result = await _controller.GetFolders(whiteSpacePrefix);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFolders_WithNullPrefix_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetFolders(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFolders_LimitsResultsToFive()
    {
        // Arrange
        var prefix = @"C:\Slides\";
        var folders = new List<string>
        {
            @"C:\Slides\A\",
            @"C:\Slides\B\",
            @"C:\Slides\C\",
            @"C:\Slides\D\",
            @"C:\Slides\E\",
            @"C:\Slides\F\",
            @"C:\Slides\G\"
        };

        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(folders));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        var returnedFolders = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(5, returnedFolders.Count());
    }

    [Fact]
    public async Task GetFolders_FewerThanFiveFolders_ReturnsAll()
    {
        // Arrange
        var prefix = @"C:\Slides\";
        var folders = new List<string>
        {
            @"C:\Slides\A\",
            @"C:\Slides\B\"
        };

        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(folders));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        var returnedFolders = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(2, returnedFolders.Count());
    }

    [Fact]
    public async Task GetFolders_EmptyFoldersList_ReturnsOkWithEmptyArray()
    {
        // Arrange
        var prefix = @"C:\Empty\";
        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(new List<string>()));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        Assert.NotNull(okResult.Value);
        var returnedFolders = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Empty(returnedFolders);
    }

    [Fact]
    public async Task GetFolders_ServiceException_PropagatesException()
    {
        // Arrange
        var prefix = @"C:\Slides\";
        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(
            Task.FromException<List<string>>(new UnauthorizedAccessException("Access denied")));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetFolders(prefix));
        Assert.Equal("Access denied", exception.Message);
    }

    [Fact]
    public async Task GetFolders_ReturnsCorrectActionResultType()
    {
        // Arrange
        var prefix = @"C:\Slides\";
        var folders = new List<string> { @"C:\Slides\Presentations\" };
        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(folders));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<List<string>>>(result);
    }

    [Fact]
    public async Task GetFolders_PreservesFolderOrderFromService()
    {
        // Arrange
        var prefix = @"C:\Data\";
        var folders = new List<string>
        {
            @"C:\Data\Alpha\",
            @"C:\Data\Beta\",
            @"C:\Data\Gamma\"
        };

        _mockFolderService.GetAccessibleFoldersAsync(prefix).Returns(Task.FromResult(folders));

        // Act
        var result = await _controller.GetFolders(prefix);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result;
        var returnedFolders = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value).ToList();
        Assert.Equal(3, returnedFolders.Count);
        Assert.Equal(@"C:\Data\Alpha\", returnedFolders[0]);
        Assert.Equal(@"C:\Data\Beta\", returnedFolders[1]);
        Assert.Equal(@"C:\Data\Gamma\", returnedFolders[2]);
    }

    #endregion
}
