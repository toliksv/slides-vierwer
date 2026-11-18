using System.Text.Json.Serialization;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Dtos;

/// <summary>
/// Результат полного анализа слайда/плёнки, включающий анализ цвета, физические дефекты, оценку качества и рекомендуемые коррекции.
/// </summary>
public class FilmAnalysisResult
{
    [JsonPropertyOrder(0)]
    public ColorDegradationAnalysis Color { get; set; } = new();

    [JsonPropertyOrder(1)]
    public PhysicalDefectAnalysis Physical { get; set; } = new();

    [JsonPropertyOrder(2)]
    public QualityAssessment Quality { get; set; } = new();

    [JsonPropertyOrder(3)]
    public CaptureOneAdjustments Corrections { get; set; } = new();

    [JsonPropertyOrder(4)]
    public string SourceFilePath { get; set; } = string.Empty;

    [JsonPropertyOrder(5)]
    public int Width { get; set; }

    [JsonPropertyOrder(6)]
    public int Height { get; set; }

    [JsonPropertyOrder(7)]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyOrder(8)]
    public List<string> OptimizationLog { get; set; } = new();

    public FilmAnalysisResult()
    {
    }
}
