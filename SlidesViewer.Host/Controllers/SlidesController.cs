using Microsoft.AspNetCore.Mvc;
using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Host.Services;

namespace SlidesViewer.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlidesController : ControllerBase
{
    private readonly ISlideService _slideService;
    private readonly IFolderService _folderService;

    public SlidesController(ISlideService slideService, IFolderService folderService)
    {
        _slideService = slideService ?? throw new ArgumentNullException(nameof(slideService));
        _folderService = folderService ?? throw new ArgumentNullException(nameof(folderService));
    }

    /// <summary>
    /// Получает категории слайдов для указанной папки с подсчётом метрик.
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<GetCategoriesResponse>> GetCategories([FromBody] GetCategoriesRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.FolderPath))
            return BadRequest("FolderPath is required");

        var response = await _slideService.GetCategories(request.FolderPath);
        return Ok(response);
    }

    /// <summary>
    /// Возвращает список подпапок по заданному префиксу пути (для autocomplete).
    /// Исключает системные, скрытые и защищённые папки. Возвращает до 5 результатов.
    /// </summary>
    [HttpGet("folders")]
    public async Task<ActionResult<List<string>>> GetFolders([FromQuery] string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return BadRequest("prefix is required");

        var folders = await _folderService.GetAccessibleFoldersAsync(prefix).ConfigureAwait(false);
        return Ok(folders.Take(5).ToList());
    }
}
