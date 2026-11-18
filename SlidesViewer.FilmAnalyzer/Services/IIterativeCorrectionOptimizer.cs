using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Services;

/// <summary>
/// Iterative correction optimizer that uses a closed-loop process:
/// [ROI Analysis] -> [Calculate Adjustments] -> [Virtual Apply on Preview] -> [Re-Analyze] -> [Damped Parameter Adjustment]
/// </summary>
public interface IIterativeCorrectionOptimizer
{
    CaptureOneAdjustments Optimize(
        Mat source16BitImage,
        CaptureOneAdjustments initialAdjustments,
        bool isNegative,
        out List<string> iterationLog);
}
