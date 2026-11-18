using Microsoft.AspNetCore.Mvc;
using SlidesViewer.CorrectionWorker.Contracts.Analyze;
using SlidesViewer.Host.Contracts.Analysis;
using SlidesViewer.Host.Services;

namespace SlidesViewer.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilmAnalysisController : ControllerBase
{
    private readonly IAnalysisMessagePublisher _messagePublisher;
    private readonly ILogger<FilmAnalysisController> _logger;

    public FilmAnalysisController(
        IAnalysisMessagePublisher messagePublisher,
        ILogger<FilmAnalysisController> logger)
    {
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Принимает путь к TIFF-файлу плёночного скана и отправляет запрос на анализ в Kafka.
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeAsync(
        [FromForm] FilmAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SlideFilePath))
            return BadRequest("Путь к файлу не указан");

        var extension = Path.GetExtension(request.SlideFilePath).ToLowerInvariant();
        if (extension != ".tif" && extension != ".tiff")
        {
            return BadRequest("Поддерживаются только файлы TIFF");
        }

        try
        {
            var slideRequest = new SlideAnalysisRequested(request.SlideFilePath);

            await _messagePublisher.PublishAsync(slideRequest, cancellationToken);

            _logger.LogInformation(
                "Film analysis requested for file: {FilePath}",
                request.SlideFilePath);

            return Ok(new { FilePath = request.SlideFilePath, Message = "Анализ инициирован" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting film analysis for file: {FilePath}", request.SlideFilePath);
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }
}
