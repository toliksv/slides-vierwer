using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Common;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Services.Adjustments;
using SlidesViewer.FilmAnalyzer.Services.QualityAssessment;

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
    private readonly IDetailAdjustmentService _detailAdjustment;
    private readonly INoiseReductionAdjustmentService _noiseReductionAdjustment;
    private readonly IIterativeCorrectionOptimizer _optimizer;

    public FilmScannerAnalyzer(
        IColorDegradationAnalyzer colorAnalyzer,
        IPhysicalDefectAnalyzer physicalAnalyzer,
        IQualityAssessmentService qualityAssessment,
        ICaptureOneStyleGenerator styleGenerator,
        IDetailAdjustmentService? detailAdjustment = null,
        INoiseReductionAdjustmentService? noiseReductionAdjustment = null,
        IIterativeCorrectionOptimizer? optimizer = null)
    {
        _colorAnalyzer = colorAnalyzer;
        _physicalAnalyzer = physicalAnalyzer;
        _qualityAssessment = qualityAssessment;
        _styleGenerator = styleGenerator;
        _detailAdjustment = detailAdjustment ?? new DetailAdjustmentService();
        _noiseReductionAdjustment = noiseReductionAdjustment ?? new NoiseReductionAdjustmentService();
        _optimizer = optimizer ?? throw new ArgumentNullException(nameof(optimizer));
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
        bool isNegative = result.Color.MediumType == FilmMediumType.ColorNegative90s;
        result.Corrections = _optimizer.Optimize(image, result.Corrections, isNegative, out var optLog);
        result.OptimizationLog = optLog;

        // Step 6: Generate Capture One style XML
        result.CaptureOneStyleXml = _styleGenerator.GenerateStyle(result);

        result.AnalyzedAt = DateTime.UtcNow;

        return result;
    }

    private CaptureOneAdjustments GenerateCorrections(FilmAnalysisResult result)
    {
        var adjustments = new CaptureOneAdjustments();

        _detailAdjustment.Apply(adjustments, result);
        _noiseReductionAdjustment.Apply(adjustments, result);

        return adjustments;
    }
}
