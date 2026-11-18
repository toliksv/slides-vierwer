using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки шумоподавления: яркостной и цветовой шум, восстановление деталей.
/// </summary>
public class NoiseReductionAdjustment
{
    [JsonPropertyOrder(0)]
    public double Luminance { get; set; }

    [JsonPropertyOrder(1)]
    public double Color { get; set; }

    [JsonPropertyOrder(2)]
    public double LuminanceDetail { get; set; }

    [JsonPropertyOrder(3)]
    public double ColorDetail { get; set; }

    public NoiseReductionAdjustment()
    {
    }
}
