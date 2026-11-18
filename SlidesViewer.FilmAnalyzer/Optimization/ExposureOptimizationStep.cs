using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

public class ExposureOptimizationStep : IOptimizationStep
{
    public bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage)
    {
        double maxClipHi = Math.Max(metrics.ClipHiR, Math.Max(metrics.ClipHiG, metrics.ClipHiB));

        if (maxClipHi > 0.005 && adjustments.Exposure.Exposure > -1.5)
        {
            adjustments.Exposure.Exposure = Math.Max(-1.5, adjustments.Exposure.Exposure - 0.15);
            logMessage = $"Exposure reduced to {adjustments.Exposure.Exposure:F2} EV due to highlight clipping ({maxClipHi:P1})";
            return true;
        }

        double maxClipLo = Math.Max(metrics.ClipLoR, Math.Max(metrics.ClipLoG, metrics.ClipLoB));

        if (maxClipLo > 0.05 && maxClipHi < 0.002 && adjustments.Exposure.Exposure < 1.5)
        {
            adjustments.Exposure.Exposure = Math.Min(1.5, adjustments.Exposure.Exposure + 0.15);
            logMessage = $"Exposure increased to {adjustments.Exposure.Exposure:F2} EV due to shadow clipping ({maxClipLo:P1})";
            return true;
        }

        logMessage = string.Empty;
        return false;
    }
}
