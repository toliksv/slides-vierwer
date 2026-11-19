using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Common;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class DtoTests
{
    [Fact]
    public void FilmAnalysisResult_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var result = new FilmAnalysisResult();

        // Assert
        Assert.NotNull(result.Color);
        Assert.NotNull(result.Physical);
        Assert.NotNull(result.Quality);
        Assert.NotNull(result.Corrections);
        Assert.Equal(string.Empty, result.SourceFilePath);
        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
        Assert.NotEqual(default, result.AnalyzedAt);
        Assert.NotNull(result.OptimizationLog);
        Assert.Empty(result.OptimizationLog);
        Assert.Null(result.CaptureOneStyleXml);
    }

    [Fact]
    public void FilmAnalysisResult_CanSetAllProperties()
    {
        // Arrange
        var result = new FilmAnalysisResult
        {
            SourceFilePath = "test.png",
            Width = 1920,
            Height = 1080,
            Color = new ColorDegradationAnalysis { MediumType = FilmMediumType.Slide80s },
            Physical = new PhysicalDefectAnalysis { ScratchCount = 5 },
            Quality = new QualityAssessment { OverallScore = 85.5 },
            Corrections = new CaptureOneAdjustments(),
            OptimizationLog = new List<string> { "log1" },
            CaptureOneStyleXml = "<SL></SL>"
        };

        // Act & Assert
        Assert.Equal("test.png", result.SourceFilePath);
        Assert.Equal(1920, result.Width);
        Assert.Equal(1080, result.Height);
        Assert.Equal(FilmMediumType.Slide80s, result.Color.MediumType);
        Assert.Equal(5, result.Physical.ScratchCount);
        Assert.Equal(85.5, result.Quality.OverallScore);
        Assert.Single(result.OptimizationLog);
        Assert.Equal("<SL></SL>", result.CaptureOneStyleXml);
    }

    [Fact]
    public void ColorDegradationAnalysis_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var analysis = new ColorDegradationAnalysis();

        // Assert
        Assert.Equal(0, analysis.R_Mean);
        Assert.Equal(0, analysis.G_Mean);
        Assert.Equal(0, analysis.B_Mean);
        Assert.Equal(ColorCastType.None, analysis.Cast);
        Assert.Equal(0, analysis.CastIntensity);
        Assert.Equal(0, analysis.OverallFading);
        Assert.Equal(0, analysis.OverallSaturation);
        Assert.Equal(FilmMediumType.Unknown, analysis.MediumType);
        Assert.NotNull(analysis.R_Histogram);
        Assert.NotNull(analysis.G_Histogram);
        Assert.NotNull(analysis.B_Histogram);
    }

    [Fact]
    public void PhysicalDefectAnalysis_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var analysis = new PhysicalDefectAnalysis();

        // Assert
        Assert.Equal(0, analysis.ScratchCount);
        Assert.Equal(0, analysis.DustCount);
        Assert.Equal(0, analysis.HazeLevel);
        Assert.Equal(0, analysis.BaseFogDensity);
        Assert.Equal(0, analysis.LocalContrast);
        Assert.False(analysis.HasBorder);
    }

    [Fact]
    public void QualityAssessment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var assessment = new QualityAssessment();

        // Assert
        Assert.Equal(0, assessment.OverallScore);
        Assert.Equal(0, assessment.ColorScore);
        Assert.Equal(0, assessment.SharpnessScore);
        Assert.Equal(0, assessment.DefectScore);
        Assert.Equal(0, assessment.DynamicRangeScore);
    }

    [Fact]
    public void CaptureOneAdjustments_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adjustments = new CaptureOneAdjustments();

        // Assert
        Assert.NotNull(adjustments.Exposure);
        Assert.NotNull(adjustments.WhiteBalance);
        Assert.NotNull(adjustments.ToneCurve);
        Assert.NotNull(adjustments.ColorCalibration);
        Assert.NotNull(adjustments.HdrToneMapping);
        Assert.NotNull(adjustments.Detail);
        Assert.NotNull(adjustments.NoiseReduction);
    }

    [Fact]
    public void ExposureAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new ExposureAdjustment();

        // Assert
        Assert.Equal(0, adj.Exposure);
        Assert.Equal(0, adj.BlackLevelRed);
        Assert.Equal(0, adj.BlackLevelGreen);
        Assert.Equal(0, adj.BlackLevelBlue);
        Assert.Equal(0, adj.Highlight);
        Assert.Equal(0, adj.Shadow);
        Assert.Equal(0, adj.Contrast);
        Assert.Equal(0, adj.ShadowR);
        Assert.Equal(0, adj.ShadowG);
        Assert.Equal(0, adj.ShadowB);
        Assert.Equal(0, adj.HighlightR);
        Assert.Equal(0, adj.HighlightG);
        Assert.Equal(0, adj.HighlightB);
    }

    [Fact]
    public void WhiteBalanceAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new WhiteBalanceAdjustment();

        // Assert
        Assert.Equal(0, adj.ColorTemperature);
        Assert.Equal(0, adj.Tint);
    }

    [Fact]
    public void ToneCurveAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new ToneCurveAdjustment();

        // Assert
        Assert.Equal("S-Curve", adj.CurveType);
        Assert.Empty(adj.RedPoints);
        Assert.Empty(adj.GreenPoints);
        Assert.Empty(adj.BluePoints);
        Assert.Empty(adj.LuminancePoints);
    }

    [Fact]
    public void DetailAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new DetailAdjustment();

        // Assert
        Assert.Equal(0, adj.Sharpness);
        Assert.Equal(0, adj.EdgeEnhancement);
        Assert.Equal(0, adj.MicroContrast);
    }

    [Fact]
    public void NoiseReductionAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new NoiseReductionAdjustment();

        // Assert
        Assert.Equal(0, adj.Luminance);
        Assert.Equal(0, adj.Color);
        Assert.Equal(0, adj.LuminanceDetail);
        Assert.Equal(0, adj.ColorDetail);
    }

    [Fact]
    public void ColorCalibrationAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new ColorCalibrationAdjustment();

        // Assert
        Assert.Equal(0, adj.ShadowTint);
        Assert.Equal(0, adj.RedHue);
        Assert.Equal(0, adj.RedSaturation);
        Assert.Equal(0, adj.GreenHue);
        Assert.Equal(0, adj.GreenSaturation);
        Assert.Equal(0, adj.BlueHue);
        Assert.Equal(0, adj.BlueSaturation);
        Assert.Equal(0, adj.MidtoneRedGain);
        Assert.Equal(0, adj.MidtoneGreenGain);
        Assert.Equal(0, adj.MidtoneBlueGain);
    }

    [Fact]
    public void HdrToneMappingAdjustment_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var adj = new HdrToneMappingAdjustment();

        // Assert
        Assert.Equal(0, adj.Contrast);
        Assert.Equal(0, adj.HighlightRecovery);
        Assert.Equal(0, adj.ShadowRecovery);
        Assert.Equal(0, adj.Brightness);
        Assert.Equal(0, adj.Saturation);
    }

    [Fact]
    public void HistogramData_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var histogram = new HistogramData();

        // Assert
        Assert.Empty(histogram.Values);
        Assert.Equal(0, histogram.Bins);
        Assert.Equal(0, histogram.MinValue);
        Assert.Equal(0, histogram.MaxValue);
        Assert.Equal(0, histogram.Mean);
        Assert.Equal(0, histogram.StdDev);
    }

    [Fact]
    public void HistogramData_Constructor_SetsAllProperties()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };

        // Act
        var histogram = new HistogramData(values, 256, 0, 65535, 32768, 1000);

        // Assert
        Assert.Same(values, histogram.Values);
        Assert.Equal(256, histogram.Bins);
        Assert.Equal(0, histogram.MinValue);
        Assert.Equal(65535, histogram.MaxValue);
        Assert.Equal(32768, histogram.Mean);
        Assert.Equal(1000, histogram.StdDev);
    }

    [Fact]
    public void FrameMetrics_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var metrics = new Dtos.Optimizations.FrameMetrics();

        // Assert
        Assert.Equal(0, metrics.ClipHiR);
        Assert.Equal(0, metrics.ClipHiG);
        Assert.Equal(0, metrics.ClipHiB);
        Assert.Equal(0, metrics.ClipLoR);
        Assert.Equal(0, metrics.ClipLoG);
        Assert.Equal(0, metrics.ClipLoB);
        Assert.Equal(0, metrics.P50_R);
        Assert.Equal(0, metrics.P50_G);
        Assert.Equal(0, metrics.P50_B);
        Assert.Equal(0, metrics.P95_R);
        Assert.Equal(0, metrics.P95_G);
        Assert.Equal(0, metrics.P95_B);
        Assert.Equal(0, metrics.P02_R);
        Assert.Equal(0, metrics.P02_G);
        Assert.Equal(0, metrics.P02_B);
        Assert.Equal(0, metrics.P05_R);
        Assert.Equal(0, metrics.P05_G);
        Assert.Equal(0, metrics.P05_B);
        Assert.Equal(0, metrics.P98_R);
        Assert.Equal(0, metrics.P98_G);
        Assert.Equal(0, metrics.P98_B);
        Assert.Equal(0, metrics.P01_R);
        Assert.Equal(0, metrics.P01_G);
        Assert.Equal(0, metrics.P01_B);
        Assert.Equal(0, metrics.P99_R);
        Assert.Equal(0, metrics.P99_G);
        Assert.Equal(0, metrics.P99_B);
        Assert.False(metrics.IsNegative);
    }
}
