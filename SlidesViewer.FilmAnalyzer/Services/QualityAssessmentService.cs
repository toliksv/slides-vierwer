using Microsoft.Extensions.Logging;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services;

public interface IQualityAssessmentService
{
    QualityAssessment Assess(ColorDegradationAnalysis color, PhysicalDefectAnalysis physical);
}

public class QualityAssessmentService : IQualityAssessmentService
{
    private readonly ILogger<QualityAssessmentService> _logger;

    public QualityAssessmentService(ILogger<QualityAssessmentService> logger)
    {
        _logger = logger;
    }

    public QualityAssessment Assess(ColorDegradationAnalysis color, PhysicalDefectAnalysis physical)
    {
        _logger.LogDebug("Starting quality assessment for {SourceType}", color.MediumType);

        var assessment = new QualityAssessment();

        // Color score (0-100)
        assessment.ColorScore = CalculateColorScore(color);

        // Sharpness score based on local contrast
        assessment.SharpnessScore = CalculateSharpnessScore(physical);

        // Defect score based on scratches and dust
        assessment.DefectScore = CalculateDefectScore(physical);

        // Dynamic range score based on histogram spread
        assessment.DynamicRangeScore = CalculateDynamicRangeScore(color);

        // Overall score (weighted average)
        assessment.OverallScore = 
            assessment.ColorScore * 0.35 +
            assessment.SharpnessScore * 0.20 +
            assessment.DefectScore * 0.25 +
            assessment.DynamicRangeScore * 0.20;

        // Round to 2 decimal places
        assessment.OverallScore = Math.Round(assessment.OverallScore, 2);
        assessment.ColorScore = Math.Round(assessment.ColorScore, 2);
        assessment.SharpnessScore = Math.Round(assessment.SharpnessScore, 2);
        assessment.DefectScore = Math.Round(assessment.DefectScore, 2);
        assessment.DynamicRangeScore = Math.Round(assessment.DynamicRangeScore, 2);

        _logger.LogInformation(
            "Quality assessment complete: Overall={Overall:F1} Color={Color:F1} Sharpness={Sharpness:F1} Defects={Defects:F1} DynRange={DynRange:F1}",
            assessment.OverallScore, assessment.ColorScore, assessment.SharpnessScore,
            assessment.DefectScore, assessment.DynamicRangeScore);

        return assessment;
    }

    private double CalculateColorScore(ColorDegradationAnalysis color)
    {
        double score = 100.0;

        // Penalize for color cast
        score -= color.CastIntensity * 40.0;

        // Penalize for fading
        score -= color.OverallFading * 30.0;

        // Penalize for low saturation
        score -= (1.0 - color.OverallSaturation) * 20.0;

        // Penalize for channel imbalance
        double meanChannel = (color.R_Mean + color.G_Mean + color.B_Mean) / 3.0;
        if (meanChannel > 0)
        {
            double rImbalance = Math.Abs(color.R_Mean - meanChannel) / meanChannel;
            double gImbalance = Math.Abs(color.G_Mean - meanChannel) / meanChannel;
            double bImbalance = Math.Abs(color.B_Mean - meanChannel) / meanChannel;
            
            double maxImbalance = Math.Max(rImbalance, Math.Max(gImbalance, bImbalance));
            score -= maxImbalance * 30.0;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateSharpnessScore(PhysicalDefectAnalysis physical)
    {
        // Based on local contrast (higher = sharper)
        // Typical clear film: 0.3-0.8, hazy: 0.0-0.2
        double score = physical.LocalContrast * 100.0;

        // Penalize for haze
        score -= physical.HazeLevel * 30.0;

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateDefectScore(PhysicalDefectAnalysis physical)
    {
        double score = 100.0;

        // Penalize for scratches (significant damage)
        score -= Math.Min(50, physical.ScratchCount * 3.0);

        // Penalize for dust (less severe)
        score -= Math.Min(20, physical.DustCount * 0.5);

        // Penalize for base fog
        score -= physical.BaseFogDensity * 20.0;

        return Math.Max(0, Math.Min(100, score));
    }

    private double CalculateDynamicRangeScore(ColorDegradationAnalysis color)
    {
        // Calculate dynamic range from histogram statistics
        double totalStdDev = color.R_StdDev + color.G_StdDev + color.B_StdDev;
        double avgStdDev = totalStdDev / 3.0;

        // Normalize: good dynamic range has std dev around 5000-10000 in 16-bit
        // Score based on how close to ideal
        double idealStdDev = 7500.0;
        double deviation = Math.Abs(avgStdDev - idealStdDev) / idealStdDev;
        
        double score = 100.0 - (deviation * 50.0);

        return Math.Max(0, Math.Min(100, score));
    }
}
