using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

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
