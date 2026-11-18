using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

public class LevelsOptimizationStep : IOptimizationStep
{
    public bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage)
    {
        bool changed = false;
        string? msg = null;

        // Fix shadow haze: use P05 (5th percentile) for real dark density
        double minP05 = Math.Min(metrics.P05_R, Math.Min(metrics.P05_G, metrics.P05_B));
        if (minP05 > 0.015)
        {
            // Clamp shadow to 0.04..0.16 for slides
            double target = Math.Clamp(minP05 * 0.6, 0.04, 0.16);
            adjustments.Exposure.Shadow = Math.Clamp(adjustments.Exposure.Shadow + target, 0.04, 0.16);
            msg = $"Shadow raised to {adjustments.Exposure.Shadow:F2} to clear shadow haze (P05={minP05:P2})";
            changed = true;
        }

        // Per-channel shadow points to remove color veil in shadows
        // Calculate individual black point for each channel based on P02
        double shadowR = Math.Clamp(metrics.P02_R * 0.6, 0.02, 0.16);
        double shadowG = Math.Clamp(metrics.P02_G * 0.6, 0.02, 0.16);
        double shadowB = Math.Clamp(metrics.P02_B * 0.6, 0.02, 0.16);
        // Overall shadow stays small to preserve depth
        double rgbShadow = Math.Clamp((shadowR + shadowG + shadowB) / 3.0, 0.02, 0.05);

        adjustments.Exposure.ShadowR = shadowR;
        adjustments.Exposure.ShadowG = shadowG;
        adjustments.Exposure.ShadowB = shadowB;

        // Add S-contrast to restore midtone density
        adjustments.Exposure.Contrast = Math.Clamp(adjustments.Exposure.Contrast + 3.0, 8.0, 22.0);

        // Fix highlight clipping: pull Highlight toward max P98 — clamp no lower than 0.94
        double maxP98 = Math.Max(metrics.P98_R, Math.Max(metrics.P98_G, metrics.P98_B));
        if (maxP98 > 0.86)
        {
            double target = Math.Min(0.99, Math.Max(0.94, maxP98));
            adjustments.Exposure.Highlight = Math.Clamp(adjustments.Exposure.Highlight + (target - adjustments.Exposure.Highlight) * 0.3, 0.94, 0.99);
            if (msg is null) msg = $"Highlight adjusted to {adjustments.Exposure.Highlight:F2} (P98={maxP98:P2})";
            else msg += $"; Highlight {adjustments.Exposure.Highlight:F2}";
            changed = true;
        }

        if (changed)
        {
            logMessage = msg!;
            return true;
        }
        logMessage = string.Empty;
        return false;
    }
}
