using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки тоновой кривой: тип кривой и контрольные точки для красных, зелёных, синих каналов и светлоты.
/// </summary>
public class ToneCurveAdjustment
{
    [JsonPropertyOrder(0)]
    public string CurveType { get; set; } = "S-Curve";

    [JsonPropertyOrder(1)]
    public int[] RedPoints { get; set; } = Array.Empty<int>();

    [JsonPropertyOrder(2)]
    public int[] GreenPoints { get; set; } = Array.Empty<int>();

    [JsonPropertyOrder(3)]
    public int[] BluePoints { get; set; } = Array.Empty<int>();

    [JsonPropertyOrder(4)]
    public int[] LuminancePoints { get; set; } = Array.Empty<int>();

    public ToneCurveAdjustment()
    {
    }
}
