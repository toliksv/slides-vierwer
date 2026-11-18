using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Настройки белого баланса: цветовая температура и оттенок (tint).
/// </summary>
public class WhiteBalanceAdjustment
{
    [JsonPropertyOrder(0)]
    public double ColorTemperature { get; set; }

    [JsonPropertyOrder(1)]
    public double Tint { get; set; }

    public WhiteBalanceAdjustment()
    {
    }
}
