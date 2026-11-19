using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services.Adjustments;

public class DetailAdjustmentService : IDetailAdjustmentService
{
    public void Apply(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
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
}
