using Microsoft.Extensions.Logging;
using NSubstitute;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Common;
using SlidesViewer.FilmAnalyzer.Services.QualityAssessment;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class QualityAssessmentServiceTests
{
    private readonly ILogger<QualityAssessmentService> _logger;
    private readonly QualityAssessmentService _service;

    public QualityAssessmentServiceTests()
    {
        _logger = Substitute.For<ILogger<QualityAssessmentService>>();
        _service = new QualityAssessmentService(_logger);
    }

    [Fact]
    public void Assess_ReturnsNonNullQualityAssessment()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<QualityAssessment>(result);
    }

    [Fact]
    public void Assess_OverallScoreIsBetween0And100()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.InRange(result.OverallScore, 0, 100);
    }

    [Fact]
    public void Assess_ColorScoreIsBetween0And100()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.InRange(result.ColorScore, 0, 100);
    }

    [Fact]
    public void Assess_SharpnessScoreIsBetween0And100()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.InRange(result.SharpnessScore, 0, 100);
    }

    [Fact]
    public void Assess_DefectScoreIsBetween0And100()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.InRange(result.DefectScore, 0, 100);
    }

    [Fact]
    public void Assess_DynamicRangeScoreIsBetween0And100()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.InRange(result.DynamicRangeScore, 0, 100);
    }

    [Fact]
    public void Assess_OverallScoreIsWeightedAverageOfSubScores()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert - verify weights: 0.35 * Color + 0.20 * Sharpness + 0.25 * Defect + 0.20 * DynamicRange
        double expected = result.ColorScore * 0.35 +
                          result.SharpnessScore * 0.20 +
                          result.DefectScore * 0.25 +
                          result.DynamicRangeScore * 0.20;

        Assert.Equal(Math.Round(expected, 2), result.OverallScore);
    }

    [Fact]
    public void Assess_ScoresAreRoundedToTwoDecimalPlaces()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert - verify scores are reasonable and rounded
        Assert.InRange(result.OverallScore, 0, 100);
        Assert.InRange(result.ColorScore, 0, 100);
        Assert.InRange(result.SharpnessScore, 0, 100);
        Assert.InRange(result.DefectScore, 0, 100);
        Assert.InRange(result.DynamicRangeScore, 0, 100);
    }

    [Fact]
    public void Assess_WithHighQualityImage_ReturnsHighScores()
    {
        // Arrange - high quality image: no cast, no fading, good saturation, no defects, good contrast
        var color = new ColorDegradationAnalysis
        {
            Cast = ColorCastType.None,
            CastIntensity = 0,
            OverallFading = 0,
            OverallSaturation = 0.9,
            R_Mean = 32000,
            G_Mean = 32000,
            B_Mean = 32000,
            R_StdDev = 7500,
            G_StdDev = 7500,
            B_StdDev = 7500
        };
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 0,
            DustCount = 0,
            HazeLevel = 0,
            BaseFogDensity = 0,
            LocalContrast = 0.8
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.OverallScore > 70, $"Overall score should be high for quality image, got {result.OverallScore}");
        Assert.True(result.ColorScore > 80, $"Color score should be high, got {result.ColorScore}");
        Assert.True(result.SharpnessScore > 70, $"Sharpness score should be high, got {result.SharpnessScore}");
        Assert.True(result.DefectScore >= 100, $"Defect score should be 100 for no defects, got {result.DefectScore}");
    }

    [Fact]
    public void Assess_WithPoorQualityImage_ReturnsLowScores()
    {
        // Arrange - poor quality: strong color cast, high fading, many defects, high haze
        var color = new ColorDegradationAnalysis
        {
            Cast = ColorCastType.Magenta,
            CastIntensity = 1.0,
            OverallFading = 1.0,
            OverallSaturation = 0.0,
            R_Mean = 1000,
            G_Mean = 60000,
            B_Mean = 1000,
            R_StdDev = 1000,
            G_StdDev = 1000,
            B_StdDev = 1000
        };
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 20,
            DustCount = 40,
            HazeLevel = 1.0,
            BaseFogDensity = 1.0,
            LocalContrast = 0.05
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.OverallScore < 50, $"Overall score should be low for poor image, got {result.OverallScore}");
        Assert.True(result.ColorScore < 30, $"Color score should be very low, got {result.ColorScore}");
        Assert.True(result.DefectScore < 30, $"Defect score should be very low, got {result.DefectScore}");
    }

    [Fact]
    public void Assess_WithManyScratches_ReducesDefectScore()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 30,
            DustCount = 0,
            HazeLevel = 0,
            BaseFogDensity = 0,
            LocalContrast = 0.5
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert - each scratch reduces score by 3, max 50 points reduction
        // 30 scratches = 90 reduction capped at 50
        Assert.True(result.DefectScore <= 50, $"Defect score should be reduced by many scratches, got {result.DefectScore}");
    }

    [Fact]
    public void Assess_WithManyDust_ReducesDefectScore()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 0,
            DustCount = 50,
            HazeLevel = 0,
            BaseFogDensity = 0,
            LocalContrast = 0.5
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert - each dust reduces score by 0.5, max 20 points reduction
        // 50 dust = 25 reduction capped at 20
        Assert.True(result.DefectScore <= 80, $"Defect score should be reduced by dust, got {result.DefectScore}");
    }

    [Fact]
    public void Assess_LoggerLogDebugIsCalled()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        _service.Assess(color, physical);

        // Assert - verify logger was called (NSubstitute tracks calls automatically)
        _logger.Received(1).Log(
            Arg.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Debug),
            Arg.Any<Microsoft.Extensions.Logging.EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object?, Exception?, string>>());
    }

    [Fact]
    public void Assess_LoggerLogInformationIsCalled()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = CreatePhysicalAnalysis();

        // Act
        _service.Assess(color, physical);

        // Assert - verify logger was called
        _logger.Received(1).Log(
            Arg.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Information),
            Arg.Any<Microsoft.Extensions.Logging.EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object?, Exception?, string>>());
    }

    [Fact]
    public void Assess_WithHighBaseFogDensity_ReducesDefectScore()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 0,
            DustCount = 0,
            HazeLevel = 0,
            BaseFogDensity = 0.5,
            LocalContrast = 0.5
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert - base fog density * 20 = 10 point reduction
        Assert.True(result.DefectScore < 100, $"Defect score should be reduced by base fog, got {result.DefectScore}");
    }

    [Fact]
    public void Assess_WithHighHaze_ReducesSharpnessScore()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 0,
            DustCount = 0,
            HazeLevel = 1.0,
            BaseFogDensity = 0,
            LocalContrast = 0.5
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert - haze reduces sharpness score
        Assert.True(result.SharpnessScore < 50, $"Sharpness score should be reduced by haze, got {result.SharpnessScore}");
    }

    [Fact]
    public void Assess_WithLowLocalContrast_ReducesSharpnessScore()
    {
        // Arrange
        var color = CreateColorAnalysis();
        var physical = new PhysicalDefectAnalysis
        {
            ScratchCount = 0,
            DustCount = 0,
            HazeLevel = 0,
            BaseFogDensity = 0,
            LocalContrast = 0.01
        };

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.SharpnessScore < 50, $"Sharpness score should be low with low contrast, got {result.SharpnessScore}");
    }

    [Fact]
    public void Assess_WithIdealStdDev_ReturnsHighDynamicRangeScore()
    {
        // Arrange - ideal std dev is 7500
        var color = new ColorDegradationAnalysis
        {
            R_StdDev = 7500,
            G_StdDev = 7500,
            B_StdDev = 7500
        };
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.DynamicRangeScore > 90, $"Dynamic range score should be high with ideal std dev, got {result.DynamicRangeScore}");
    }

    [Fact]
    public void Assess_WithNonIdealStdDev_ReturnsLowerDynamicRangeScore()
    {
        // Arrange
        var color = new ColorDegradationAnalysis
        {
            R_StdDev = 100,
            G_StdDev = 100,
            B_StdDev = 100
        };
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.DynamicRangeScore < 60, $"Dynamic range score should be low with non-ideal std dev, got {result.DynamicRangeScore}");
    }

    [Fact]
    public void Assess_ChannelsImbalance_ReducesColorScore()
    {
        // Arrange - very imbalanced channels
        var color = new ColorDegradationAnalysis
        {
            R_Mean = 1000,
            G_Mean = 60000,
            B_Mean = 1000,
            Cast = ColorCastType.None,
            CastIntensity = 0,
            OverallFading = 0,
            OverallSaturation = 0.5
        };
        var physical = CreatePhysicalAnalysis();

        // Act
        var result = _service.Assess(color, physical);

        // Assert
        Assert.True(result.ColorScore < 80, $"Color score should be reduced by channel imbalance, got {result.ColorScore}");
    }

    private static ColorDegradationAnalysis CreateColorAnalysis()
    {
        return new ColorDegradationAnalysis
        {
            R_Mean = 30000,
            G_Mean = 30000,
            B_Mean = 30000,
            R_StdDev = 7000,
            G_StdDev = 7000,
            B_StdDev = 7000,
            Cast = ColorCastType.None,
            CastIntensity = 0.1,
            OverallFading = 0.1,
            OverallSaturation = 0.7,
            MediumType = Dtos.Common.FilmMediumType.Slide80s
        };
    }

    private static PhysicalDefectAnalysis CreatePhysicalAnalysis()
    {
        return new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5,
            HasBorder = false
        };
    }
}
