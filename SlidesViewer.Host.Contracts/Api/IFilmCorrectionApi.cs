using RestEase;
using SlidesViewer.FilmAnalyzer.Dtos;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.Host.Contracts.Api;

/// <summary>
/// REST-клиент для Correction Worker API (анализ плёночных сканов).
/// </summary>
public interface IFilmCorrectionApi
{
    /// <summary>
    /// Получить статус задачи по ID.
    /// </summary>
    [Get("/api/analysis/status/{jobId}")]
    Task<CorrectionJobStatusResponse> GetStatus([Path] string jobId);

    /// <summary>
    /// Получить результат анализа по ID.
    /// </summary>
    [Get("/api/analysis/result/{jobId}")]
    Task<FilmAnalysisResult?> GetResult([Path] string jobId);

    /// <summary>
    /// Получить Capture One скрипт по ID.
    /// </summary>
    [Get("/api/analysis/script/{jobId}")]
    Task<string> GetScript([Path] string jobId);
}
