using System.Globalization;
using System.Text;
using System.Xml;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

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
        AddElement("ColorBalanceMidtone", FormatColorBalance3(1.0, 1.0, 1.0));
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

        // --- Highlight / Shadow ---
        AddElement("Highlight", GenerateHighlightValues(analysis));
        AddElement("HighlightRecoveryEx", FormatValue(analysis.Corrections.HdrToneMapping.HighlightRecovery));
        AddElement("Midtone", "0;0;0;0");
        AddElement("Moire", "0;8");
        AddElement("NoiseDetailsAmount", FormatValue(analysis.Corrections.NoiseReduction.LuminanceDetail));
        AddElement("NrAmount", FormatValue(analysis.Corrections.NoiseReduction.Luminance));
        AddElement("Saturation", FormatValue(analysis.Corrections.HdrToneMapping.Saturation));
        AddElement("Shadow", GenerateShadowValues(analysis));
        AddElement("ShadowRecovery", FormatValue(analysis.Corrections.HdrToneMapping.ShadowRecovery));
        AddElement("SurfaceSmoothnessAmount", FormatValue(0));
        AddElement("TargetHighlight", "1;1;1;1");
        AddElement("TargetShadow", "0;0;0;0");

        // --- Detail (USM) ---
        AddElement("UsmAmount", FormatValue(analysis.Corrections.Detail.Sharpness));
        AddElement("UsmHaloControl", FormatValue(0.0));
        AddElement("UsmRadius", FormatValue(analysis.Corrections.Detail.EdgeEnhancement));
        AddElement("UsmThreshold", FormatValue(analysis.Corrections.Detail.MicroContrast));

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

        // Only add (0,0) manually if the first point is NOT already (0,0)
        if (!(points.Length >= 2 && points[0] == 0 && points[1] == 0))
        {
            parts.Add("0,0");
        }

        // Skip the first pair if it was already (0,0)
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
        if (points.Length < 2 || !(points[^2] == 65535 && points[^1] == 65535))
        {
            parts.Add("1,1");
        }

        return parts.Count > 0 ? string.Join(";", parts) : "0,0;1,1";
    }

    private string GenerateHighlightValues(FilmAnalysisResult analysis)
    {
        double hr = analysis.Corrections.Exposure.HighlightR;
        double hg = analysis.Corrections.Exposure.HighlightG;
        double hb = analysis.Corrections.Exposure.HighlightB;

        // Fallback to uniform Highlight if per-channel values not set
        if (hr <= 0 && hg <= 0 && hb <= 0)
        {
            double h = analysis.Corrections.Exposure.Highlight;
            if (h <= 0) h = 0.95;
            return $"1;{FormatValue(h)};{FormatValue(h)};{FormatValue(h)}";
        }

        // Per-channel highlight: RGB;R;G;B
        return $"1;{FormatValue(hr)};{FormatValue(hg)};{FormatValue(hb)}";
    }

    /// <summary>
    /// Generates Shadow values (0;R;G;B) for the Shadow element.
    /// Uses per-channel ShadowR/ShadowG/ShadowB when available to remove color veil in shadows.
    /// </summary>
    private string GenerateShadowValues(FilmAnalysisResult analysis)
    {
        double sr = analysis.Corrections.Exposure.ShadowR;
        double sg = analysis.Corrections.Exposure.ShadowG;
        double sb = analysis.Corrections.Exposure.ShadowB;

        // Fallback to uniform Shadow if per-channel values not set
        if (sr <= 0 && sg <= 0 && sb <= 0)
        {
            double s = analysis.Corrections.Exposure.Shadow;
            if (s <= 0) s = 0.05;
            return $"0;{FormatValue(s)};{FormatValue(s)};{FormatValue(s)}";
        }

        // Per-channel shadow: 0;R;G;B — individual black point per R/G/B removes color veil
        return $"0;{FormatValue(sr)};{FormatValue(sg)};{FormatValue(sb)}";
    }
}
