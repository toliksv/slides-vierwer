using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

public class WhiteBalanceOptimizationStep : IOptimizationStep
{
    public bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage)
    {
        double minTemp, maxTemp, maxTint;

        if (!metrics.IsNegative)
        {
            minTemp = 4900; maxTemp = 6100; maxTint = 5.0;
        }
        else
        {
            minTemp = 4600; maxTemp = 6400; maxTint = 8.0;
        }

        double blueRedDiff = metrics.P95_B - metrics.P95_R;

        if (Math.Abs(blueRedDiff) > 0.035)
        {
            adjustments.WhiteBalance.ColorTemperature = Math.Clamp(
                adjustments.WhiteBalance.ColorTemperature + (blueRedDiff > 0 ? 150.0 : -150.0),
                minTemp, maxTemp);
            logMessage = $"White balance temperature adjusted to {adjustments.WhiteBalance.ColorTemperature:F0}K";
            return true;
        }

        double greenCast = metrics.P95_G - (metrics.P95_R + metrics.P95_B) / 2.0;

        if (Math.Abs(greenCast) > 0.03)
        {
            adjustments.WhiteBalance.Tint = Math.Clamp(
                adjustments.WhiteBalance.Tint + (greenCast > 0 ? 1.5 : -1.5),
                -maxTint, maxTint);
            logMessage = $"White balance tint adjusted to {adjustments.WhiteBalance.Tint:F2}";
            return true;
        }

        logMessage = string.Empty;
        return false;
    }
}
