using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Analysis;

/// <summary>
/// Анализ физических дефектов: количество царапин и пыли, уровень дымки, плотность фоновой вуали, локальный контраст и наличие рамки.
/// </summary>
public class PhysicalDefectAnalysis
{
    [JsonPropertyOrder(0)]
    public int ScratchCount { get; set; }

    [JsonPropertyOrder(1)]
    public int DustCount { get; set; }

    [JsonPropertyOrder(2)]
    public double HazeLevel { get; set; }

    [JsonPropertyOrder(3)]
    public double BaseFogDensity { get; set; }

    [JsonPropertyOrder(4)]
    public double LocalContrast { get; set; }

    [JsonPropertyOrder(5)]
    public bool HasBorder { get; set; }

    public PhysicalDefectAnalysis()
    {
    }
}
