using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

public class ToneCurveOptimizationStep : IOptimizationStep
{
    public bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage)
    {
        var lum = adjustments.ToneCurve.LuminancePoints;
        if (lum.Length > 1)
        {
            for (int i = 1; i < lum.Length; i++)
            {
                if (lum[i] <= lum[i - 1])
                    lum[i] = (int)(i * 65535.0 / (lum.Length - 1));
            }
        }

        double avgMid = (metrics.P50_R + metrics.P50_G + metrics.P50_B) / 3.0;
        int midIdx = Math.Max(1, lum.Length / 2);
        int midY = lum[midIdx];
        string? msg = null;

        if (TryAdjustChannel(avgMid, metrics.P50_R, metrics.ClipHiR, adjustments.ToneCurve.RedPoints, midY, 980, out msg) && msg is not null)
        { /* ok */ }
        else if (TryAdjustChannel(avgMid, metrics.P50_G, metrics.ClipHiG, adjustments.ToneCurve.GreenPoints, midY, 980, out msg) && msg is not null)
        { /* ok */ }
        else if (TryAdjustChannel(avgMid, metrics.P50_B, metrics.ClipHiB, adjustments.ToneCurve.BluePoints, midY, 980, out msg) && msg is not null)
        { /* ok */ }

        adjustments.ColorCalibration.MidtoneRedGain = 1.0;
        adjustments.ColorCalibration.MidtoneGreenGain = 1.0;
        adjustments.ColorCalibration.MidtoneBlueGain = 1.0;

        if (msg is not null) { logMessage = msg; return true; }
        logMessage = string.Empty;
        return false;
    }

    private bool TryAdjustChannel(double avgMid, double p50, double clipHi, int[] points, int midY, int step, out string? msg)
    {
        if (points.Length <= 1) { msg = null; return false; }
        int idx = Math.Max(1, points.Length / 2);
        double deviation = (avgMid - p50) / avgMid;

        if (deviation > 0.07 && clipHi < 0.002)
        {
            points[idx] = Math.Clamp(points[idx] + step, 30800, 34735);
            msg = $"Channel curve adjusted ({deviation:P1})";
            return true;
        }
        if (clipHi > 0.005)
        {
            points[idx] = Math.Clamp(points[idx] - step, 30800, 34735);
            msg = $"Channel curve reduced due to clipping ({clipHi:P1})";
            return true;
        }
        msg = null;
        return false;
    }
}
