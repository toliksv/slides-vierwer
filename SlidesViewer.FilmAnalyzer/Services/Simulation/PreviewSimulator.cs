using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Services.Simulation;

/// <summary>
/// Симуляция превью — применяет коррекции к изображению для виртуального отображения результата.
/// </summary>
public class PreviewSimulator : IPreviewSimulator
{
    public Mat Simulate(Mat source, CaptureOneAdjustments adj)
    {
        double expMul = Math.Pow(2.0, adj.Exposure.Exposure);
        double tempDelta = (adj.WhiteBalance.ColorTemperature - 5500.0) / 5500.0;
        double rMul = 1.0 + tempDelta * 0.7;
        double bMul = 1.0 - tempDelta * 0.7;
        double tintG = 1.0 - (adj.WhiteBalance.Tint * 0.015);

        double shadowR = Math.Clamp(adj.Exposure.ShadowR, 0.01, 0.25);
        double shadowG = Math.Clamp(adj.Exposure.ShadowG, 0.01, 0.25);
        double shadowB = Math.Clamp(adj.Exposure.ShadowB, 0.01, 0.25);
        double highlightR = Math.Clamp(adj.Exposure.HighlightR > 0 ? adj.Exposure.HighlightR : adj.Exposure.Highlight, 0.88, 1.00);
        double highlightG = Math.Clamp(adj.Exposure.HighlightG > 0 ? adj.Exposure.HighlightG : adj.Exposure.Highlight, 0.88, 1.00);
        double highlightB = Math.Clamp(adj.Exposure.HighlightB > 0 ? adj.Exposure.HighlightB : adj.Exposure.Highlight, 0.88, 1.00);

        Cv2.Split(source, out var channels);
        for (int c = 0; c < 3; c++)
        {
            channels[c].GetArray<float>(out var arr);
            int[] pts = c == 0 ? adj.ToneCurve.BluePoints : c == 1 ? adj.ToneCurve.GreenPoints : adj.ToneCurve.RedPoints;
            double midY = pts.Length >= 4 ? pts[3] / 65535.0 : 0.5;

            double shadow = c == 0 ? shadowB : c == 1 ? shadowG : shadowR;
            double highlight = c == 0 ? highlightB : c == 1 ? highlightG : highlightR;

            for (int i = 0; i < arr.Length; i++)
            {
                double v = arr[i] * expMul;
                if (c == 0) v *= rMul;
                else if (c == 2) v *= bMul;
                if (c == 1) v *= tintG;
                v = Math.Clamp((v - shadow) / (highlight - shadow), 0.0, 1.0);
                double delta = midY - 0.5;
                v = Math.Clamp(v + delta * 4.0 * v * (1.0 - v), 0.0, 1.0);
                arr[i] = (float)v;
            }
            channels[c].SetArray(arr);
        }
        var outMat = new Mat();
        Cv2.Merge(channels, outMat);
        return outMat;
    }
}
