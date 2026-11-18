using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

public class ToneCurveOptimizationStep : IOptimizationStep
{
    // Limit to 2 activations across the entire optimization cycle
    private int _activationCount;
    private const int MaxActivations = 2;

    public bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage)
    {
        var lum = adjustments.ToneCurve.LuminancePoints;
        if (lum.Length > 1)
        {
            for (int i = 2; i < lum.Length; i++)
            {
                if (lum[i] <= lum[i - 1])
                    lum[i] = (int)(i * 65535.0 / (lum.Length - 1));
            }
        }

        // Account for ColorTemperature and Tint effects on each channel
        double tempDelta = (adjustments.WhiteBalance.ColorTemperature - 5500.0) / 5500.0;
        double rMul = 1.0 + tempDelta * 0.7;
        double bMul = 1.0 - tempDelta * 0.7;
        double tintG = 1.0 - (adjustments.WhiteBalance.Tint * 0.015);

        // Adjusted P50 per channel after CT/Tint
        double adjP50R = metrics.P50_R * rMul;
        double adjP50G = metrics.P50_G * tintG;
        double adjP50B = metrics.P50_B * bMul;
        double adjAvgMid = (adjP50R + adjP50G + adjP50B) / 3.0;

        int midIdx = Math.Max(1, lum.Length / 2);
        int midY = lum[midIdx];
        string? msg = null;

        // Only apply if we haven't exceeded max activations
        if (_activationCount < MaxActivations)
        {
            if (TryAdjustChannel(adjAvgMid, adjP50R, metrics.ClipHiR, adjustments.ToneCurve.RedPoints, midY, 450, out msg) && msg is not null)
            { /* ok */ }
            else if (TryAdjustChannel(adjAvgMid, adjP50G, metrics.ClipHiG, adjustments.ToneCurve.GreenPoints, midY, 450, out msg) && msg is not null)
            { /* ok */ }
            else if (TryAdjustChannel(adjAvgMid, adjP50B, metrics.ClipHiB, adjustments.ToneCurve.BluePoints, midY, 450, out msg) && msg is not null)
            { /* ok */ }
        }

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

        double deviation = (avgMid - p50) / avgMid;

        if (deviation > 0.12 && clipHi < 0.002)
        {
            // P50 < avgMid → channel is dark → increase curve
            points[midIdxFor(points)] = Math.Clamp(points[midIdxFor(points)] + step, 30800, 35400);
            _activationCount++;
            msg = $"Channel curve adjusted ({deviation:P1}, activations={_activationCount}/{MaxActivations})";
            return true;
        }
        if (clipHi > 0.005)
        {
            // P50 > avgMid → channel is bright → decrease curve
            points[midIdxFor(points)] = Math.Clamp(points[midIdxFor(points)] - step, 30800, 35400);
            _activationCount++;
            msg = $"Channel curve reduced due to clipping ({clipHi:P1}, activations={_activationCount}/{MaxActivations})";
            return true;
        }
        msg = null;
        return false;
    }

    private int midIdxFor(int[] points) => Math.Max(1, points.Length / 2);
}
