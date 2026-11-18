using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

public class ColorDegradationAnalysis
{
    [JsonPropertyOrder(0)]
    public double R_Mean { get; set; }

    [JsonPropertyOrder(1)]
    public double G_Mean { get; set; }

    [JsonPropertyOrder(2)]
    public double B_Mean { get; set; }

    [JsonPropertyOrder(3)]
    public double R_StdDev { get; set; }

    [JsonPropertyOrder(4)]
    public double G_StdDev { get; set; }

    [JsonPropertyOrder(5)]
    public double B_StdDev { get; set; }

    [JsonPropertyOrder(6)]
    public ColorCastType Cast { get; set; }

    [JsonPropertyOrder(7)]
    public double CastIntensity { get; set; }

    [JsonPropertyOrder(8)]
    public double R_Fading { get; set; }

    [JsonPropertyOrder(9)]
    public double G_Fading { get; set; }

    [JsonPropertyOrder(10)]
    public double B_Fading { get; set; }

    [JsonPropertyOrder(11)]
    public double OverallFading { get; set; }

    [JsonPropertyOrder(12)]
    public double OverallSaturation { get; set; }

    [JsonPropertyOrder(13)]
    public HistogramData R_Histogram { get; set; } = new();

    [JsonPropertyOrder(14)]
    public HistogramData G_Histogram { get; set; } = new();

    [JsonPropertyOrder(15)]
    public HistogramData B_Histogram { get; set; } = new();

    [JsonPropertyOrder(16)]
    public double EstimatedWhiteBalance_R { get; set; }

    [JsonPropertyOrder(17)]
    public double EstimatedWhiteBalance_B { get; set; }

    [JsonPropertyOrder(18)]
    public double SuggestedWhiteBalance_R { get; set; }

    [JsonPropertyOrder(19)]
    public double SuggestedWhiteBalance_B { get; set; }

    public ColorDegradationAnalysis()
    {
    }
}
