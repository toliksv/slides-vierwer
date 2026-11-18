using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

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

    public FilmAnalysisResult()
    {
    }
}
