using System.Globalization;
using System.Text;
using System.Xml;
using SlidesViewer.FilmAnalyzer.Dtos;

namespace SlidesViewer.FilmAnalyzer.Generators;

public class CaptureOneStyleGenerator : ICaptureOneStyleGenerator
{
    public string GenerateStyle(FilmAnalysisResult analysis)
    {
        var doc = new XmlDocument();
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = doc.CreateElement("SL");
        root.SetAttribute("Engine", "1300");
        doc.AppendChild(root);

        // Helper to add an element
        void AddElement(string key, string value)
        {
            var elem = doc.CreateElement("E");
            elem.SetAttribute("K", key);
            elem.SetAttribute("V", value);
            root.AppendChild(elem);
        }

        // Name
        var fileName = Path.GetFileNameWithoutExtension(analysis.SourceFilePath);
        AddElement("Name", fileName);
        // UUID with dashes: 8-4-4-4-12 format
        var rawUuid = Guid.NewGuid().ToString("N").ToUpperInvariant();
        AddElement("UUID", $"{rawUuid[..8]}-{rawUuid[8..12]}-{rawUuid[12..16]}-{rawUuid[16..20]}-{rawUuid[20..]}");
        AddElement("StyleSource", "Styles");

        // --- Exposure ---
        AddElement("AIDenoiseAmount", FormatValue(0));
        AddElement("BlackRecovery", FormatValue(0.0));
        AddElement("Brightness", FormatValue(analysis.Corrections.HdrToneMapping.Brightness));
        AddElement("Clarity", FormatValue(0.0));
        AddElement("ClarityMethod", FormatValue(3));
        AddElement("ClarityStructure", FormatValue(0.0));
        AddElement("CleanLongExposureAmount", FormatValue(0));
        AddElement("CnrAmount", FormatValue(analysis.Corrections.NoiseReduction.Color));
        AddElement("ColorBalance", FormatColorBalance3(1.0, 1.0, 1.0));
        AddElement("ColorBalanceHighlight", FormatColorBalance3(1.0, 1.0, 1.0));
        AddElement("ColorBalanceMidtone", FormatColorBalance3(
            analysis.Corrections.ColorCalibration.MidtoneRedGain,
            analysis.Corrections.ColorCalibration.MidtoneGreenGain,
            analysis.Corrections.ColorCalibration.MidtoneBlueGain));
        AddElement("ColorBalanceShadow", FormatColorBalance3(1.0, 1.0, 1.0));
        AddElement("Contrast", FormatValue(analysis.Corrections.HdrToneMapping.Contrast));
        AddElement("DehazeAmount", FormatValue(0));
        AddElement("DehazeColor", "0;0;0");
        AddElement("DenoiseMethod", FormatValue(0));
        AddElement("Exposure", FormatValue(analysis.Corrections.Exposure.Exposure));
        AddElement("FilmGrainAmount", FormatValue(0));
        AddElement("FilmGrainDensity", FormatValue(0));
        AddElement("FilmGrainGranularity", FormatValue(50));
        AddElement("FilmGrainType", FormatValue(0));

        // --- Tone Curves ---
        AddElement("GradationCurve", FormatToneCurve(analysis.Corrections.ToneCurve.LuminancePoints));
        AddElement("GradationCurveRed", FormatToneCurve(analysis.Corrections.ToneCurve.RedPoints));
        AddElement("GradationCurveGreen", FormatToneCurve(analysis.Corrections.ToneCurve.GreenPoints));
        AddElement("GradationCurveBlue", FormatToneCurve(analysis.Corrections.ToneCurve.BluePoints));

        // --- Color Calibration (per-channel gains & shadow tint) ---
        AddElement("ColorBalanceShadow", FormatColorBalance3(1.0, 1.0, 1.0));

        // --- Highlight / Shadow ---
        AddElement("Highlight", GenerateHighlightValues(analysis));
        AddElement("HighlightRecoveryEx", FormatValue(analysis.Corrections.HdrToneMapping.HighlightRecovery));
        AddElement("Midtone", "0;0;0;0");
        AddElement("Moire", "0;8");
        AddElement("Name", fileName);
        AddElement("NoiseDetailsAmount", FormatValue(analysis.Corrections.NoiseReduction.LuminanceDetail));
        AddElement("NrAmount", FormatValue(analysis.Corrections.NoiseReduction.Luminance));
        AddElement("Saturation", FormatValue(analysis.Corrections.HdrToneMapping.Saturation));
        AddElement("Shadow", GenerateShadowValues(analysis));
        AddElement("ShadowRecovery", FormatValue(analysis.Corrections.HdrToneMapping.ShadowRecovery));
        AddElement("StyleSource", "Styles");
        AddElement("SurfaceSmoothnessAmount", FormatValue(0));
        AddElement("TargetHighlight", "1;1;1;1");
        AddElement("TargetShadow", "0;0;0;0");

        // --- Detail (USM) ---
        AddElement("UsmAmount", FormatValue(analysis.Corrections.Detail.Sharpness));
        AddElement("UsmHaloControl", FormatValue(0.0));
        AddElement("UsmRadius", FormatValue(analysis.Corrections.Detail.EdgeEnhancement));
        AddElement("UsmThreshold", FormatValue(analysis.Corrections.Detail.MicroContrast));

        // --- UUID ---
        var uuidValue = $"{rawUuid[..8]}-{rawUuid[8..12]}-{rawUuid[12..16]}-{rawUuid[16..20]}-{rawUuid[20..]}";
        AddElement("UUID", uuidValue);

        // --- White Balance ---
        AddElement("WhiteBalanceTemperature", FormatValue(analysis.Corrections.WhiteBalance.ColorTemperature));
        AddElement("WhiteBalanceTint", FormatValue(analysis.Corrections.WhiteBalance.Tint));
        AddElement("WhiteRecovery", FormatValue(0));

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t",
            Encoding = new UTF8Encoding(false),
            OmitXmlDeclaration = false
        };

        using var sw = new System.IO.StringWriter();
        using var writer = XmlWriter.Create(sw, settings);
        doc.WriteContentTo(writer);
        writer.Flush();

        return sw.ToString();
    }

    private string FormatValue(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    private string FormatValue(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private string FormatColorBalance3(double r, double g, double b)
    {
        return $"{r.ToString("R", CultureInfo.InvariantCulture)};{g.ToString("R", CultureInfo.InvariantCulture)};{b.ToString("R", CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Formats tone curve points from 16-bit (0-65535) to 0-1 range for Capture One.
    /// Ensures the curve starts at (0,0) and ends at (1,1).
    /// </summary>
    private string FormatToneCurve(int[] points)
    {
        if (points == null || points.Length == 0)
        {
            return "0,0;1,1";
        }

        var parts = new List<string>();

        // Check if the first point is already (0,0) to avoid duplication
        if (points.Length >= 2 && points[0] == 0 && points[1] == 0)
        {
            parts.Add("0,0");
        }
        else
        {
            // Always ensure start at (0,0)
            parts.Add("0,0");
        }

        // Convert 16-bit points (0-65535) to 0-1 range, skip the first if already added
        int startIndex = (points.Length >= 2 && points[0] == 0 && points[1] == 0) ? 2 : 0;

        for (int i = startIndex; i < points.Length; i += 2)
        {
            if (i + 1 < points.Length)
            {
                double x = points[i] / 65535.0;
                double y = points[i + 1] / 65535.0;
                // Clamp to [0, 1]
                x = Math.Max(0, Math.Min(1, x));
                y = Math.Max(0, Math.Min(1, y));
                parts.Add($"{x.ToString("R", CultureInfo.InvariantCulture)},{y.ToString("R", CultureInfo.InvariantCulture)}");
            }
        }

        // Always end at (1,1) if not already present
        if (parts.Count < 2 || !(parts[^1].StartsWith("1") || parts[^1].StartsWith("1.0")))
        {
            parts.Add("1,1");
        }

        return parts.Count > 0 ? string.Join(";", parts) : "0,0;1,1";
    }

    /// <summary>
    /// Generates per-channel RGB tone curves based on color analysis.
    /// Creates a simple 3-point curve (black, midtone, white) for each channel.
    /// The midtone Y value is adjusted based on channel gain to correct color cast.
    /// </summary>
    private string GenerateChannelCurve(double channelGain, int[] luminancePoints)
    {
        // Find the midtone X from the luminance curve
        double midtoneX = 0.5;
        double midtoneY = 0.5;
        
        if (luminancePoints != null && luminancePoints.Length >= 4)
        {
            // Find the point closest to 50% in the luminance curve
            double targetX = 0.5;
            for (int i = 2; i < luminancePoints.Length; i += 2)
            {
                double x = luminancePoints[i] / 65535.0;
                if (Math.Abs(x - targetX) < 0.01)
                {
                    midtoneX = x;
                    midtoneY = Math.Max(0, Math.Min(1, luminancePoints[i + 1] / 65535.0));
                    break;
                }
            }
        }

        // Apply channel gain to the midtone Y
        double adjustedY = midtoneY * channelGain;
        adjustedY = Math.Max(0, Math.Min(1, adjustedY));

        return $"0,0;{midtoneX.ToString("R", CultureInfo.InvariantCulture)},{adjustedY.ToString("R", CultureInfo.InvariantCulture)};1,1";
    }

    private string GenerateHighlightValues(FilmAnalysisResult analysis)
    {
        double h = analysis.Corrections.Exposure.Highlight;
        // Default to 0.95 if not set
        if (h <= 0) h = 0.95;
        
        return $"{h.ToString("R", CultureInfo.InvariantCulture)};{h.ToString("R", CultureInfo.InvariantCulture)};{h.ToString("R", CultureInfo.InvariantCulture)};1.0";
    }

    /// <summary>
    /// Generates Shadow values (R,G,B,Alpha) for the Shadow element.
    /// Uses ExposureAdjustment.Shadow when available.
    /// </summary>
    private string GenerateShadowValues(FilmAnalysisResult analysis)
    {
        double s = analysis.Corrections.Exposure.Shadow;
        // Default to 0.05 if not set
        if (s <= 0) s = 0.05;
        
        return $"{s.ToString("R", CultureInfo.InvariantCulture)};{s.ToString("R", CultureInfo.InvariantCulture)};{s.ToString("R", CultureInfo.InvariantCulture)};0";
    }
}
