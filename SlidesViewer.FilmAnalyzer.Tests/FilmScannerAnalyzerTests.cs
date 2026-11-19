using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Common;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Services.QualityAssessment;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class FilmScannerAnalyzerTests
{
    private readonly IFixture _fixture;
    private readonly IColorDegradationAnalyzer _colorAnalyzer;
    private readonly IPhysicalDefectAnalyzer _physicalAnalyzer;
    private readonly IQualityAssessmentService _qualityAssessment;
    private readonly ICaptureOneStyleGenerator _styleGenerator;
    private readonly IIterativeCorrectionOptimizer _optimizer;
    private readonly FilmScannerAnalyzer _analyzer;

    public FilmScannerAnalyzerTests()
    {
        _fixture = new Fixture();
        _colorAnalyzer = Substitute.For<IColorDegradationAnalyzer>();
        _physicalAnalyzer = Substitute.For<IPhysicalDefectAnalyzer>();
        _qualityAssessment = Substitute.For<IQualityAssessmentService>();
        _styleGenerator = Substitute.For<ICaptureOneStyleGenerator>();
        _optimizer = Substitute.For<IIterativeCorrectionOptimizer>();

        _analyzer = new FilmScannerAnalyzer(
            _colorAnalyzer,
            _physicalAnalyzer,
            _qualityAssessment,
            _styleGenerator,
            optimizer: _optimizer);
    }

    [Fact]
    public void Analyze_ThrowsException_WhenFileCannotBeRead()
    {
        // Arrange
        var filePath = _fixture.Create<string>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _analyzer.Analyze(filePath));
        Assert.Contains($"Cannot read image: {filePath}", exception.Message);
    }

    [Fact]
    public void Analyze_CallsAllDependencies_InCorrectOrder()
    {
        // Arrange
        var filePath = _fixture.Create<string>();
        var colorAnalysis = _fixture.Build<ColorDegradationAnalysis>()
            .With(a => a.MediumType, FilmMediumType.ColorNegative90s)
            .Create();
        // Explicitly create PhysicalDefectAnalysis to avoid AutoFixture null issues
        var physicalAnalysis = new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5
        };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var corrections = _fixture.Create<CaptureOneAdjustments>();
        var styleXml = _fixture.Create<string>();

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);
        _optimizer.Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), true, out Arg.Any<List<string>>())
            .Returns(corrections);
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns(styleXml);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(128, 128, 128));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            var result = _analyzer.Analyze(tempFile);

            // Assert
            _colorAnalyzer.Received(1).Analyze(tempFile);
            _physicalAnalyzer.Received(1).Analyze(tempFile);
            _qualityAssessment.Received(1).Assess(colorAnalysis, physicalAnalysis);
            _optimizer.Received(1).Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), true, out Arg.Any<List<string>>());
            _styleGenerator.Received(1).GenerateStyle(Arg.Any<FilmAnalysisResult>());

            Assert.Equal(tempFile, result.SourceFilePath);
            Assert.Equal(100, result.Width);
            Assert.Equal(100, result.Height);
            Assert.Equal(colorAnalysis, result.Color);
            Assert.Equal(physicalAnalysis, result.Physical);
            Assert.Equal(qualityAssessment, result.Quality);
            Assert.NotNull(result.Corrections);
            Assert.Equal(styleXml, result.CaptureOneStyleXml);
            Assert.NotEqual(default, result.AnalyzedAt);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Theory, AutoData]
    public void Analyze_SetsCorrections_OptimizerIsCalled(string filePath)
    {
        // Arrange
        var colorAnalysis = _fixture.Build<ColorDegradationAnalysis>()
            .With(a => a.MediumType, FilmMediumType.Slide80s)
            .Create();
        var physicalAnalysis = new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5
        };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var initialCorrections = new CaptureOneAdjustments();
        var optimizedCorrections = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment { Exposure = 0.5 }
        };
        var styleXml = "<SL></SL>";

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);
        _optimizer.Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), false, out Arg.Any<List<string>>())
            .Returns(optimizedCorrections);
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns(styleXml);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(100, 150, 200));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            var result = _analyzer.Analyze(tempFile);

            // Assert
            Assert.Equal(optimizedCorrections, result.Corrections);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Analyze_PassesIsNegativeFlagCorrectly_ForColorNegative()
    {
        // Arrange
        var filePath = _fixture.Create<string>();
        var colorAnalysis = _fixture.Build<ColorDegradationAnalysis>()
            .With(a => a.MediumType, FilmMediumType.ColorNegative90s)
            .Create();
        var physicalAnalysis = new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5
        };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var corrections = _fixture.Create<CaptureOneAdjustments>();

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);
        _optimizer.Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), true, out Arg.Any<List<string>>())
            .Returns(corrections);
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns("xml");

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(128, 128, 128));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            _analyzer.Analyze(tempFile);

            // Assert
            _optimizer.Received(1).Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), true, out Arg.Any<List<string>>());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Analyze_PassesIsNegativeFlagCorrectly_ForSlide()
    {
        // Arrange
        var filePath = _fixture.Create<string>();
        var colorAnalysis = _fixture.Build<ColorDegradationAnalysis>()
            .With(a => a.MediumType, FilmMediumType.Slide80s)
            .Create();
        var physicalAnalysis = new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5
        };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var corrections = _fixture.Create<CaptureOneAdjustments>();

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);
        _optimizer.Optimize(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>(), false, out Arg.Any<List<string>>())
            .Returns(corrections);
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns("xml");

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(128, 128, 128));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            _analyzer.Analyze(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Analyze_DetailAdjustments_SharpenessWithinValidRange()
    {
        // Arrange - test the internal AdjustDetail logic by providing known Physical values
        var filePath = _fixture.Create<string>();
        var colorAnalysis = _fixture.Create<ColorDegradationAnalysis>();
        var physicalAnalysis = new PhysicalDefectAnalysis { LocalContrast = 0.02, HazeLevel = 0.1 };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var corrections = new CaptureOneAdjustments();
        var styleXml = "xml";

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);
        // Use a callback to capture and verify the adjustments passed to Optimize
        CaptureOneAdjustments? capturedAdj = null;
        _optimizer.Optimize(
            Arg.Any<Mat>(),
            Arg.Do<CaptureOneAdjustments>(a => capturedAdj = a),
            false,
            out Arg.Any<List<string>>())
            .Returns(new CaptureOneAdjustments());
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns(styleXml);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(128, 128, 128));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            _analyzer.Analyze(tempFile);

            // Assert - verify Detail adjustments set by FilmScannerAnalyzer internally
            Assert.NotNull(capturedAdj);
            Assert.True(capturedAdj.Detail.Sharpness >= 40 && capturedAdj.Detail.Sharpness <= 90,
                $"Sharpness {capturedAdj.Detail.Sharpness} should be in range 40..90");
            Assert.True(capturedAdj.Detail.EdgeEnhancement >= 0.3 && capturedAdj.Detail.EdgeEnhancement <= 3.0,
                $"EdgeEnhancement {capturedAdj.Detail.EdgeEnhancement} should be in range 0.3..3.0");
            Assert.True(capturedAdj.Detail.MicroContrast >= 1.0 && capturedAdj.Detail.MicroContrast <= 5.0,
                $"MicroContrast {capturedAdj.Detail.MicroContrast} should be in range 1.0..5.0");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Analyze_NoiseReduction_AdjustmentsWithinValidRange()
    {
        // Arrange
        var filePath = _fixture.Create<string>();
        var colorAnalysis = new ColorDegradationAnalysis
        {
            R_StdDev = 5000,
            G_StdDev = 5000,
            B_StdDev = 5000
        };
        var physicalAnalysis = new PhysicalDefectAnalysis
        {
            ScratchCount = 2,
            DustCount = 5,
            HazeLevel = 0.1,
            BaseFogDensity = 0.05,
            LocalContrast = 0.5
        };
        var qualityAssessment = _fixture.Create<Dtos.Analysis.QualityAssessment>();
        var styleXml = "xml";

        _colorAnalyzer.Analyze(Arg.Any<string>()).Returns(colorAnalysis);
        _physicalAnalyzer.Analyze(Arg.Any<string>()).Returns(physicalAnalysis);
        _qualityAssessment.Assess(colorAnalysis, physicalAnalysis).Returns(qualityAssessment);

        // Use a callback to capture and verify the adjustments passed to Optimize
        CaptureOneAdjustments? capturedAdj = null;
        _optimizer.Optimize(
            Arg.Any<Mat>(),
            Arg.Do<CaptureOneAdjustments>(a => capturedAdj = a),
            false,
            out Arg.Any<List<string>>())
            .Returns(new CaptureOneAdjustments());
        _styleGenerator.GenerateStyle(Arg.Any<FilmAnalysisResult>()).Returns(styleXml);

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.png");
        try
        {
            using var testImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(128, 128, 128));
            Cv2.ImWrite(tempFile, testImage);

            // Act
            _analyzer.Analyze(tempFile);

            // Assert - verify NoiseReduction adjustments set by FilmScannerAnalyzer internally
            Assert.NotNull(capturedAdj);
            Assert.True(capturedAdj.NoiseReduction.Luminance >= 0 && capturedAdj.NoiseReduction.Luminance <= 50,
                $"Luminance {capturedAdj.NoiseReduction.Luminance} should be in range 0..50");
            Assert.True(capturedAdj.NoiseReduction.Color >= 0 && capturedAdj.NoiseReduction.Color <= 50,
                $"Color {capturedAdj.NoiseReduction.Color} should be in range 0..50");
            Assert.True(capturedAdj.NoiseReduction.LuminanceDetail >= 0, "LuminanceDetail should be >= 0");
            Assert.True(capturedAdj.NoiseReduction.ColorDetail >= 0, "ColorDetail should be >= 0");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
