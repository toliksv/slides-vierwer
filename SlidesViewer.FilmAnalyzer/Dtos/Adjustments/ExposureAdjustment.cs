using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки экспозиции: общая экспозиция, уровни чёрного по каналам, подсветка и тени.
/// </summary>
public class ExposureAdjustment
{
    [JsonPropertyOrder(0)]
    public double Exposure { get; set; }

    [JsonPropertyOrder(1)]
    public int BlackLevelRed { get; set; }

    [JsonPropertyOrder(2)]
    public int BlackLevelGreen { get; set; }

    [JsonPropertyOrder(3)]
    public int BlackLevelBlue { get; set; }

    // Per-channel highlight/shadow targets (R;G;B;A format in Capture One)
    [JsonPropertyOrder(4)]
    public double Highlight { get; set; }

    [JsonPropertyOrder(5)]
    public double Shadow { get; set; }

    [JsonPropertyOrder(6)]
    public double Contrast { get; set; }

    // Per-channel shadow targets (used by CaptureOneStyleGenerator for shadow tint removal)
    [JsonPropertyOrder(7)]
    public double ShadowR { get; set; }

    [JsonPropertyOrder(8)]
    public double ShadowG { get; set; }

    [JsonPropertyOrder(9)]
    public double ShadowB { get; set; }

    public ExposureAdjustment()
    {
    }
}
