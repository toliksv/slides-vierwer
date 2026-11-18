using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки детализации: резкость, усиление краёв и микроконтраст.
/// </summary>
public class DetailAdjustment
{
    [JsonPropertyOrder(0)]
    public double Sharpness { get; set; }

    [JsonPropertyOrder(1)]
    public double EdgeEnhancement { get; set; }

    [JsonPropertyOrder(2)]
    public double MicroContrast { get; set; }

    public DetailAdjustment()
    {
    }
}
