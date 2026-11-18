using Microsoft.AspNetCore.Mvc;
using RestEase;
using SlidesViewer.Host.Contracts.Api;
using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Web.ViewModels;

namespace SlidesViewer.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISlidesApi _slidesApi;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ISlidesApi slidesApi, ILogger<HomeController> logger, IConfiguration configuration)
    {
        _slidesApi = slidesApi;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("api/home/username")]
    public IActionResult GetUsername()
    {
        return Ok(Environment.UserName);
    }

    /// <summary>
    /// Прокси для автодополнения папок — перенаправляет запрос к SlidesViewer.Host.
    /// </summary>
    [HttpGet("api/home/folders")]
    public async Task<IActionResult> GetFolders([FromQuery] string prefix)
    {
        _logger.LogInformation("GetFolders called with prefix: {Prefix}", prefix);

        if (string.IsNullOrWhiteSpace(prefix))
        {
            _logger.LogWarning("GetFolders: prefix is empty");
            return BadRequest("prefix is required");
        }

        try
        {
            _logger.LogInformation("Forwarding to SlidesViewer.Host");

            var folders = await _slidesApi.GetFolders(prefix);

            _logger.LogInformation("Received {Count} folders", folders?.Count ?? 0);
            return Ok(folders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding folder request to SlidesViewer.Host");
            return StatusCode(500, $"Ошибка подключения к серверу: {ex.Message}");
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        var viewModel = new FolderScanViewModel
        {
            DefaultFolderPath = _configuration["SlidesViewer:DefaultFolderPath"] ?? string.Empty
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromForm] string folderPath)
    {
        var viewModel = new FolderScanViewModel
        {
            FolderPath = folderPath
        };

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            viewModel.ErrorMessage = "Пожалуйста, введите путь к папке.";
            return View(viewModel);
        }

        try
        {
            var response = await _slidesApi.GetCategories(new GetCategoriesRequest { FolderPath = folderPath });

            if (response != null)
            {
                viewModel.Categories = response.Categories;
                viewModel.TotalFiles = response.TotalFiles;
                viewModel.TotalCategories = response.TotalCategories;
                viewModel.TotalUniquePaths = response.TotalUniquePaths;
            }
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = $"Ошибка подключения к серверу: {ex.Message}";
        }

        return View(viewModel);
    }
}
