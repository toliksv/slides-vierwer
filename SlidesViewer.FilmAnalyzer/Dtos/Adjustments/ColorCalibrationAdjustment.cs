using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки калибровки цвета: оттенок и насыщенность для теней, красных/зелёных/синих тонов, усиление средних тонов.
/// </summary>
public class ColorCalibrationAdjustment
{
    [JsonPropertyOrder(0)]
    public double ShadowTint { get; set; }

    [JsonPropertyOrder(1)]
    public double RedHue { get; set; }

    [JsonPropertyOrder(2)]
    public double RedSaturation { get; set; }

    [JsonPropertyOrder(3)]
    public double GreenHue { get; set; }

    [JsonPropertyOrder(4)]
    public double GreenSaturation { get; set; }

    [JsonPropertyOrder(5)]
    public double BlueHue { get; set; }

    [JsonPropertyOrder(6)]
    public double BlueSaturation { get; set; }

    [JsonPropertyOrder(7)]
    public double MidtoneRedGain { get; set; }

    [JsonPropertyOrder(8)]
    public double MidtoneGreenGain { get; set; }

    [JsonPropertyOrder(9)]
    public double MidtoneBlueGain { get; set; }

    public ColorCalibrationAdjustment()
    {
    }
}
