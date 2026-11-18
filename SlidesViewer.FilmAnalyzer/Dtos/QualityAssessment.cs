using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

public class QualityAssessment
{
    [JsonPropertyOrder(0)]
    public double OverallScore { get; set; }

    [JsonPropertyOrder(1)]
    public double ColorScore { get; set; }

    [JsonPropertyOrder(2)]
    public double SharpnessScore { get; set; }

    [JsonPropertyOrder(3)]
    public double DefectScore { get; set; }

    [JsonPropertyOrder(4)]
    public double DynamicRangeScore { get; set; }

    public QualityAssessment()
    {
    }
}
