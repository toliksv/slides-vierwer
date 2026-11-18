using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

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

    public ExposureAdjustment()
    {
    }
}

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
