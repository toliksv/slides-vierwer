using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки тон-маппинга HDR: контраст, восстановление светов и теней, яркость и насыщенность.
/// </summary>
public class HdrToneMappingAdjustment
{
    [JsonPropertyOrder(0)]
    public double Contrast { get; set; }

    [JsonPropertyOrder(1)]
    public double HighlightRecovery { get; set; }

    [JsonPropertyOrder(2)]
    public double ShadowRecovery { get; set; }

    [JsonPropertyOrder(3)]
    public double Brightness { get; set; }

    [JsonPropertyOrder(4)]
    public double Saturation { get; set; }

    public HdrToneMappingAdjustment()
    {
    }
}
