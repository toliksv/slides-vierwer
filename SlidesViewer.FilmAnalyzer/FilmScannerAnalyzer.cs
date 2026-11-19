using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Common;
using SlidesViewer.FilmAnalyzer.Generators;

using SlidesViewer.FilmAnalyzer.Services;

namespace SlidesViewer.FilmAnalyzer;

public interface IFilmScannerAnalyzer
{
    FilmAnalysisResult Analyze(string filePath);
}

public class FilmScannerAnalyzer : IFilmScannerAnalyzer
{
    private readonly IColorDegradationAnalyzer _colorAnalyzer;
    private readonly IPhysicalDefectAnalyzer _physicalAnalyzer;
    private readonly IQualityAssessmentService _qualityAssessment;
    private readonly ICaptureOneStyleGenerator _styleGenerator;
    private readonly IIterativeCorrectionOptimizer _optimizer;

    public FilmScannerAnalyzer(
        IColorDegradationAnalyzer colorAnalyzer,
        IPhysicalDefectAnalyzer physicalAnalyzer,
        IQualityAssessmentService qualityAssessment,
        ICaptureOneStyleGenerator styleGenerator,
        IIterativeCorrectionOptimizer optimizer)
    {
        _colorAnalyzer = colorAnalyzer;
        _physicalAnalyzer = physicalAnalyzer;
        _qualityAssessment = qualityAssessment;
        _styleGenerator = styleGenerator;
        _optimizer = optimizer;
    }

    public FilmAnalysisResult Analyze(string filePath)
    {
        // Load image to get dimensions
        using var image = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        
        if (image.Empty())
            throw new InvalidOperationException($"Cannot read image: {filePath}");

        var result = new FilmAnalysisResult
        {
            SourceFilePath = filePath,
            Width = image.Cols,
            Height = image.Rows
        };

        // Step 1: Color degradation analysis
        result.Color = _colorAnalyzer.Analyze(filePath);

        // Step 2: Physical defect analysis
        result.Physical = _physicalAnalyzer.Analyze(filePath);

        // Step 3: Quality assessment
        result.Quality = _qualityAssessment.Assess(result.Color, result.Physical);

        // Step 4: Generate corrections
        result.Corrections = GenerateCorrections(result);

        // Step 5: Iterative optimization
        bool isNegative = (result.Color.R_StdDev + result.Color.G_StdDev + result.Color.B_StdDev) / 3.0 < 8500;
        using var optImage = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        result.Corrections = _optimizer.Optimize(optImage, result.Corrections, isNegative, out var optLog);
        result.OptimizationLog = optLog;

        result.AnalyzedAt = DateTime.UtcNow;

        return result;
    }

    private CaptureOneAdjustments GenerateCorrections(FilmAnalysisResult result)
    {
        var adjustments = new CaptureOneAdjustments();

        // Branch correction logic by detected film medium type
        switch (result.Color.MediumType)
        {
            case FilmMediumType.ColorNegative90s:
                AdjustColorNegativeCorrections(adjustments, result);
                break;
            case FilmMediumType.Slide80s:
            default:
                AdjustSlideCorrections(adjustments, result);
                break;
        }

        // Detail and noise reduction are universal
        AdjustDetail(adjustments, result);
        AdjustNoiseReduction(adjustments, result);

        return adjustments;
    }

    private void AdjustSlideCorrections(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Exposure: soft correction, range -0.6..+0.8
        AdjustExposure(adjustments, result);
        adjustments.Exposure.Exposure = Clamp(adjustments.Exposure.Exposure, -0.6, 0.8);

        // White balance: color temperature 5100..5900 K, tint -4..+4
        AdjustWhiteBalance(adjustments, result);
        adjustments.WhiteBalance.ColorTemperature = Clamp(adjustments.WhiteBalance.ColorTemperature, 5100, 5900);
        adjustments.WhiteBalance.Tint = Clamp(adjustments.WhiteBalance.Tint, -4, 4);

        // Soft S-curve for slides
        AdjustToneCurve(adjustments, result);

        // Color calibration
        AdjustColorCalibration(adjustments, result);

        // HDR tone mapping (Brightness = 0)
        AdjustHdrToneMapping(adjustments, result);
    }

    private void AdjustColorNegativeCorrections(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Exposure: wider range for negatives, -1.0..+1.2
        AdjustExposure(adjustments, result);
        adjustments.Exposure.Exposure = Clamp(adjustments.Exposure.Exposure, -1.0, 1.2);

        // White balance: broader range for negative green-brown cast
        AdjustWhiteBalance(adjustments, result);
        adjustments.WhiteBalance.ColorTemperature = Clamp(adjustments.WhiteBalance.ColorTemperature, 3500, 7500);
        adjustments.WhiteBalance.Tint = Clamp(adjustments.WhiteBalance.Tint, -15, 15);

        // Strong S-curve for color negatives
        AdjustColorNegativeToneCurve(adjustments);

        // Color calibration: compensate green-brown shadow cast
        AdjustColorCalibration(adjustments, result);

        // HDR tone mapping (Brightness = 0)
        AdjustHdrToneMapping(adjustments, result);

        // Per-channel highlight/shadow targets
        double highlightTarget = 0.90 + (adjustments.Exposure.Exposure > 0 ? 0.06 : 0.0);
        double shadowTarget = 0.08 - (adjustments.Exposure.Exposure > 0 ? 0.05 : 0.0);
        adjustments.Exposure.Highlight = Clamp(highlightTarget, 0.90, 0.96);
        adjustments.Exposure.Shadow = Clamp(shadowTarget, 0.03, 0.08);
    }

    private void AdjustExposure(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Calculate exposure adjustment based on average channel mean
        double avgMean = (result.Color.R_Mean + result.Color.G_Mean + result.Color.B_Mean) / 3.0;
        
        // Ideal mean is around 32768 (midpoint of 16-bit)
        // If mean is lower, increase exposure; if higher, decrease
        double exposureDelta = (32768.0 - avgMean) / 32768.0;
        
        // Convert to EV stops (approximately log2)
        double exposureEv = exposureDelta * 2.0; // Scale factor
        
        // Clamp to reasonable range
        adjustments.Exposure.Exposure = Math.Max(-3.0, Math.Min(3.0, exposureEv));

        // Black level estimation from base fog
        double baseFog = result.Physical.BaseFogDensity * 32768.0;
        int blackLevel = (int)Math.Max(0, Math.Min(255, baseFog / 128.0));

        adjustments.Exposure.BlackLevelRed = blackLevel;
        adjustments.Exposure.BlackLevelGreen = blackLevel;
        adjustments.Exposure.BlackLevelBlue = blackLevel;
    }

    private void AdjustWhiteBalance(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Convert white balance gains to Color Temperature and Tint
        double rGain = result.Color.SuggestedWhiteBalance_R;
        double bGain = result.Color.SuggestedWhiteBalance_B;

        // Simple approximation: convert gains to Kelvin
        // This is a simplified model; real conversion is more complex
        double ratio = rGain / bGain;
        
        // Higher ratio = warmer (more red, less blue)
        // Lower ratio = cooler (less red, more blue)
        double temperature = 6500.0 / ratio; // D65 white point reference
        
        // Clamp to reasonable range
        temperature = Math.Max(2000, Math.Min(10000, temperature));

        // Tint calculation based on green-magenta balance
        double gMean = result.Color.G_Mean;
        double avgMean = (result.Color.R_Mean + gMean + result.Color.B_Mean) / 3.0;
        double tintOffset = (gMean - avgMean) / avgMean;
        double tint = tintOffset * 100.0; // Scale to Capture One range

        adjustments.WhiteBalance.ColorTemperature = temperature;
        adjustments.WhiteBalance.Tint = Math.Max(-50, Math.Min(50, tint));
    }

    private void AdjustToneCurve(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        bool isNegative = (result.Color.R_StdDev + result.Color.G_StdDev + result.Color.B_StdDev) / 3.0 < 8500;

        // Strictly monotonic tone curve — no V-shaped inflections
        int[] points = new int[10];
        points[0] = 0;          points[1] = 0;
        points[2] = 16384;      points[3] = isNegative ? 14000 : 15800;
        points[4] = 32768;      points[5] = 32768;
        points[6] = 49152;      points[7] = isNegative ? 51500 : 49700;
        points[8] = 65535;      points[9] = 65535;

        adjustments.ToneCurve.LuminancePoints = points;
        adjustments.ToneCurve.CurveType = "S-Curve";

        GenerateChannelCurves(adjustments, result);
    }

    /// <summary>
    /// Moderate S-curve for color negatives: shadow lift and highlight compression
    /// to counter the green-brown shadow cast and low-contrast negative appearance.
    /// Uses fixed X coordinates to prevent solarization.
    /// </summary>
    private void AdjustColorNegativeToneCurve(CaptureOneAdjustments adjustments)
    {
        // Fixed monotonic X coordinates
        const int X0 = 0;
        const int X1 = 16384; // 25%
        const int X2 = 32768; // 50%
        const int X3 = 49152; // 75%
        const int X4 = 65535; // 100%

        // Moderate S-curve: 25% -> 21%, 50% -> 50%, 75% -> 79%
        int[] points = new int[10];
        points[0] = X0; points[1] = 0;
        points[2] = X1; points[3] = (int)(0.21 * 65535); // 13762
        points[4] = X2; points[5] = 32768;
        points[6] = X3; points[7] = (int)(0.79 * 65535); // 51773
        points[8] = X4; points[9] = 65535;

        // Strict monotonicity guard
        for (int i = 3; i < points.Length; i += 2)
        {
            if (points[i + 1] <= points[i - 1])
                points[i + 1] = points[i - 1] + 1;
        }

        adjustments.ToneCurve.LuminancePoints = points;
        adjustments.ToneCurve.CurveType = "Moderate S-Curve";
    }

    /// <summary>
    /// Generates per-channel RGB tone curves based on color channel means.
    /// Each channel gets a 3-point curve (black, midtone, white) where the midtone
    /// Y value is adjusted to correct color cast while preserving the S-curve shape.
    /// </summary>
    private void GenerateChannelCurves(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        double rMean = result.Color.R_Mean;
        double gMean = result.Color.G_Mean;
        double bMean = result.Color.B_Mean;
        double avgMean = (rMean + gMean + bMean) / 3.0;
        
        if (avgMean <= 0)
        {
            adjustments.ToneCurve.RedPoints = Array.Empty<int>();
            adjustments.ToneCurve.GreenPoints = Array.Empty<int>();
            adjustments.ToneCurve.BluePoints = Array.Empty<int>();
            return;
        }

        // Find the midtone point from the luminance curve
        int[] lumPoints = adjustments.ToneCurve.LuminancePoints;
        double midtoneX = 0.5;
        // Luminance midtone is now 0.5 (neutral), channel curves use it as base.
        // Channel factors are applied very subtly — WhiteBalance and ColorBalanceShadow
        // handle the main color correction. Channel curves only fine-tune.
        const double ChannelCorrectionStrength = 0.1;
        double midtoneY = 0.5;
        
        if (lumPoints != null && lumPoints.Length >= 4)
        {
            for (int i = 2; i < lumPoints.Length; i += 2)
            {
                double x = lumPoints[i] / 65535.0;
                if (Math.Abs(x - 0.5) < 0.05)
                {
                    midtoneX = x;
                    midtoneY = lumPoints[i + 1] / 65535.0;
                    break;
                }
            }
        }

        // Calculate channel adjustment factors
        // Uses grey-world principle: each channel is scaled toward the average.
        // Very subtle correction (10% of factor deviation) — main correction is in
        // WhiteBalance (Temperature/Tint) and ColorBalanceShadow (gains).
        double rFactor = avgMean / rMean;
        double gFactor = avgMean / gMean;
        double bFactor = avgMean / bMean;

        // Clamp factors to narrow range
        const double ChannelFactorMin = 0.85;
        const double ChannelFactorMax = 1.15;
        rFactor = Math.Max(ChannelFactorMin, Math.Min(ChannelFactorMax, rFactor));
        gFactor = Math.Max(ChannelFactorMin, Math.Min(ChannelFactorMax, gFactor));
        bFactor = Math.Max(ChannelFactorMin, Math.Min(ChannelFactorMax, bFactor));

        // Apply very subtle correction — channel curves fine-tune, don't correct
        double redMidY = midtoneY + (rFactor - 1.0) * ChannelCorrectionStrength;
        double greenMidY = midtoneY + (gFactor - 1.0) * ChannelCorrectionStrength;
        double blueMidY = midtoneY + (bFactor - 1.0) * ChannelCorrectionStrength;

        // Generate 3-point curves for each channel
        // Format: (0,0), (midtoneX, adjustedY), (65535, 65535)
        int[] redPoints = new int[6];
        redPoints[0] = 0;
        redPoints[1] = 0;
        redPoints[2] = (int)(midtoneX * 65535);
        redPoints[3] = (int)Math.Max(0, Math.Min(65535, redMidY * 65535));
        redPoints[4] = 65535;
        redPoints[5] = 65535;
        adjustments.ToneCurve.RedPoints = redPoints;

        int[] greenPoints = new int[6];
        greenPoints[0] = 0;
        greenPoints[1] = 0;
        greenPoints[2] = (int)(midtoneX * 65535);
        greenPoints[3] = (int)Math.Max(0, Math.Min(65535, greenMidY * 65535));
        greenPoints[4] = 65535;
        greenPoints[5] = 65535;
        adjustments.ToneCurve.GreenPoints = greenPoints;

        int[] bluePoints = new int[6];
        bluePoints[0] = 0;
        bluePoints[1] = 0;
        bluePoints[2] = (int)(midtoneX * 65535);
        bluePoints[3] = (int)Math.Max(0, Math.Min(65535, blueMidY * 65535));
        bluePoints[4] = 65535;
        bluePoints[5] = 65535;
        adjustments.ToneCurve.BluePoints = bluePoints;
    }

    private void AdjustColorCalibration(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Shadow tint based on channel imbalance in shadows
        double rDiff = result.Color.R_Mean - result.Color.G_Mean;
        double bDiff = result.Color.B_Mean - result.Color.G_Mean;
        
        // Shadow tint (magenta-green balance) — keep subtle
        adjustments.ColorCalibration.ShadowTint = Math.Max(-20, Math.Min(20, rDiff / 3000.0));

        // Grey-world white balance with tightly clamped gains and damping.
        // Gain range narrowed to 0.95..1.05 (±5%) with 0.3 damping factor
        // to prevent salmon/bleach casts on water and other artifacts.
        const double GainMin = 0.95;
        const double GainMax = 1.05;
        const double DampingFactor = 0.3;
        
        double avgMean = (result.Color.R_Mean + result.Color.G_Mean + result.Color.B_Mean) / 3.0;
        
        if (avgMean > 0)
        {
            double rawRGain = avgMean / result.Color.R_Mean;
            double rawBGain = avgMean / result.Color.B_Mean;

            double dampedRGain = 1.0 + Clamp((rawRGain - 1.0) * DampingFactor, -0.05, 0.05);
            double dampedBGain = 1.0 + Clamp((rawBGain - 1.0) * DampingFactor, -0.05, 0.05);

            adjustments.ColorCalibration.MidtoneRedGain = Clamp(dampedRGain, GainMin, GainMax);
            adjustments.ColorCalibration.MidtoneGreenGain = 1.0;
            adjustments.ColorCalibration.MidtoneBlueGain = Clamp(dampedBGain, GainMin, GainMax);
        }
        else
        {
            adjustments.ColorCalibration.MidtoneRedGain = 1.0;
            adjustments.ColorCalibration.MidtoneGreenGain = 1.0;
            adjustments.ColorCalibration.MidtoneBlueGain = 1.0;
        }

        // Minor hue/saturation adjustments based on color cast
        switch (result.Color.Cast)
        {
            case ColorCastType.Magenta:
                adjustments.ColorCalibration.GreenHue = 0;
                adjustments.ColorCalibration.GreenSaturation = -5;
                break;
            case ColorCastType.Yellow:
                adjustments.ColorCalibration.BlueHue = 0;
                adjustments.ColorCalibration.BlueSaturation = -5;
                break;
            case ColorCastType.Cyan:
                adjustments.ColorCalibration.RedHue = 0;
                adjustments.ColorCalibration.RedSaturation = -5;
                break;
        }
    }

    private void AdjustHdrToneMapping(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Brightness disabled — Exposure handles global brightness to avoid conflict
        adjustments.HdrToneMapping.Brightness = 0;

        // Contrast based on dynamic range
        double dynamicRange = result.Color.R_Histogram.MaxValue - result.Color.R_Histogram.MinValue;
        double normalizedRange = dynamicRange / 65535.0;
        adjustments.HdrToneMapping.Contrast = normalizedRange * 2.0;

        // Highlight and shadow recovery based on histogram tails
        double shadowLevel = CalculatePercentile(result.Color.R_Histogram, 5) / 65535.0;
        double highlightLevel = 1.0 - (CalculatePercentile(result.Color.R_Histogram, 95) / 65535.0);
        adjustments.HdrToneMapping.ShadowRecovery = shadowLevel * 50;
        adjustments.HdrToneMapping.HighlightRecovery = highlightLevel * 50;

        // Saturation preservation
        adjustments.HdrToneMapping.Saturation = 12.0;
    }

    private void AdjustDetail(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Sharpness (USM Amount) - Capture One range 40..90
        // LocalContrast for film typically ranges 0.02-0.3
        double sharpness = Math.Max(40, Math.Min(90, result.Physical.LocalContrast * 6000));
        adjustments.Detail.Sharpness = sharpness;
        
        // Edge enhancement (USM Radius) - typically 0.5-2.0 pixels
        double radius = Math.Max(0.3, Math.Min(3.0, 0.5 + result.Physical.LocalContrast * 5));
        adjustments.Detail.EdgeEnhancement = radius;
        
        // Micro-contrast (USM Threshold) - Capture One scale 0..10, clamp to 1.0..5.0
        double threshold = Math.Max(1.0, Math.Min(5.0, result.Physical.HazeLevel * 10));
        adjustments.Detail.MicroContrast = threshold;
    }

    private void AdjustNoiseReduction(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Noise estimation based on std dev in uniform regions
        double avgStdDev = (result.Color.R_StdDev + result.Color.G_StdDev + result.Color.B_StdDev) / 3.0;
        
        // Higher std dev in midtones suggests more noise
        double noiseLevel = avgStdDev / 10000.0; // Normalize
        
        adjustments.NoiseReduction.Luminance = Math.Min(50, noiseLevel * 30);
        adjustments.NoiseReduction.Color = Math.Min(50, noiseLevel * 20);
        adjustments.NoiseReduction.LuminanceDetail = Math.Max(0, 50 - adjustments.NoiseReduction.Luminance);
        adjustments.NoiseReduction.ColorDetail = Math.Max(0, 50 - adjustments.NoiseReduction.Color);
    }

    private double CalculatePercentile(HistogramData histogram, double percentile)
    {
        if (histogram.Values.Length == 0)
            return 0;

        double target = percentile / 100.0 * histogram.Values.Sum();
        double cumulative = 0;
        
        for (int i = 0; i < histogram.Values.Length; i++)
        {
            cumulative += histogram.Values[i];
            if (cumulative >= target)
            {
                return (i * histogram.MaxValue) / histogram.Values.Length;
            }
        }
        
        return histogram.MaxValue;
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
