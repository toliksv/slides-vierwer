using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

/// <summary>
/// Агрегированные настройки коррекции в стиле Capture One: экспозиция, белый баланс, тоновая кривая, калибровка цвета, HDR, детали и шумоподавление.
/// </summary>
public class CaptureOneAdjustments
{
    [JsonPropertyOrder(0)]
    public ExposureAdjustment Exposure { get; set; } = new();

    [JsonPropertyOrder(1)]
    public WhiteBalanceAdjustment WhiteBalance { get; set; } = new();

    [JsonPropertyOrder(2)]
    public ToneCurveAdjustment ToneCurve { get; set; } = new();

    [JsonPropertyOrder(3)]
    public ColorCalibrationAdjustment ColorCalibration { get; set; } = new();

    [JsonPropertyOrder(4)]
    public HdrToneMappingAdjustment HdrToneMapping { get; set; } = new();

    [JsonPropertyOrder(5)]
    public DetailAdjustment Detail { get; set; } = new();

    [JsonPropertyOrder(6)]
    public NoiseReductionAdjustment NoiseReduction { get; set; } = new();

    public CaptureOneAdjustments()
    {
    }
}
