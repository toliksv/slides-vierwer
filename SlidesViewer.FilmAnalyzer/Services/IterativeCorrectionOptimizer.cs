using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Utilities;

namespace SlidesViewer.FilmAnalyzer.Services;

public class IterativeCorrectionOptimizer : IIterativeCorrectionOptimizer
{
    private readonly IOptimizationStep[] _steps;

    public IterativeCorrectionOptimizer(IEnumerable<IOptimizationStep>? steps = null)
    {
        var list = steps?.ToList();
        _steps = (list is null or { Count: 0 })
            ? [new LevelsOptimizationStep(), new ExposureOptimizationStep(), new WhiteBalanceOptimizationStep(), new ToneCurveOptimizationStep()]
            : list.ToArray();
    }

    public CaptureOneAdjustments Optimize(Mat src, CaptureOneAdjustments adj, bool isNegative, out List<string> log)
    {
        using var normalized16 = ImageNormalizationUtility.NormalizeTo16BitColor(src);

        int y1 = (int)(normalized16.Rows * 0.08), y2 = (int)(normalized16.Rows * 0.84);
        int x1 = (int)(normalized16.Cols * 0.08), x2 = (int)(normalized16.Cols * 0.84);
        var cropped = normalized16[y1..y2, x1..x2].Clone();

        var sz = new Size((int)(cropped.Cols * 800.0 / Math.Max(cropped.Cols, cropped.Rows)),
                          (int)(cropped.Rows * 800.0 / Math.Max(cropped.Cols, cropped.Rows)));
        var small = new Mat();
        Cv2.Resize(cropped, small, sz);
        small.ConvertTo(small, MatType.CV_32FC3, 1.0 / 65535.0);

        log = [];
        for (int iter = 1; iter <= 6; iter++)
        {
            var sim = SimulatePreview(small, adj);
            var metrics = ComputeMetrics(sim);
            bool changed = false;
            foreach (var step in _steps)
                if (step.ApplyStep(adj, metrics, out string? msg)) { log.Add($"Iter {iter}: {msg}"); changed = true; }
            if (!changed) { log.Add($"Iter {iter}: Converged"); break; }
        }
        return adj;
    }

    private static Mat SimulatePreview(Mat img, CaptureOneAdjustments adj)
    {
        double expMul = Math.Pow(2.0, adj.Exposure.Exposure);
        double tempDelta = (adj.WhiteBalance.ColorTemperature - 5500.0) / 5500.0;
        double rMul = 1.0 + tempDelta * 0.7;
        double bMul = 1.0 - tempDelta * 0.7;
        double shadow = Math.Clamp(adj.Exposure.Shadow, 0.04, 0.16);
        double highlight = Math.Clamp(adj.Exposure.Highlight, 0.86, 0.99);
        double tintG = 1.0 - (adj.WhiteBalance.Tint * 0.015);

        Cv2.Split(img, out var channels);
        for (int c = 0; c < 3; c++)
        {
            channels[c].GetArray<float>(out var arr);
            int[] pts = c == 0 ? adj.ToneCurve.BluePoints : c == 1 ? adj.ToneCurve.GreenPoints : adj.ToneCurve.RedPoints;
            double midY = pts.Length >= 4 ? pts[3] / 65535.0 : 0.5;
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

    private static FrameMetrics ComputeMetrics(Mat mat)
    {
        var m = new FrameMetrics();
        int totalPixels = mat.Rows * mat.Cols;
        double[] p50 = new double[3], p95 = new double[3], p02 = new double[3], p05 = new double[3], p98 = new double[3];

        for (int i = 0; i < 3; i++)
        {
            Mat[] ch = null!;
            try
            {
                Cv2.Split(mat, out ch);
                using var single = ch[i];

                using var maskHiLocal = new Mat();
                Cv2.Compare(single, new Scalar(0.99), maskHiLocal, CmpType.GT);
                double clipHi = Cv2.CountNonZero(maskHiLocal) / (double)totalPixels;

                using var maskLoLocal = new Mat();
                Cv2.Compare(single, new Scalar(0.005), maskLoLocal, CmpType.LT);
                double clipLo = Cv2.CountNonZero(maskLoLocal) / (double)totalPixels;

                // OpenCV BGR order: i=0->B, i=1->G, i=2->R
                switch (i)
                {
                    case 0: m.ClipHiB = clipHi; m.ClipLoB = clipLo; break;
                    case 1: m.ClipHiG = clipHi; m.ClipLoG = clipLo; break;
                    case 2: m.ClipHiR = clipHi; m.ClipLoR = clipLo; break;
                }

                float[] range = { 0f, 1f };
                int[] histSize = { 256 };
                var hist = new Mat();
                Cv2.CalcHist(new[] { single }, new[] { 0 }, null, hist, 1, histSize, new[] { range });
                hist.GetArray<float>(out var bins);
                double cumulative = 0;
                double target02 = totalPixels * 0.02;
                double target05 = totalPixels * 0.05;
                double target50 = totalPixels * 0.50;
                double target95 = totalPixels * 0.95;
                double target98 = totalPixels * 0.98;
                for (int b = 0; b < 256; b++)
                {
                    cumulative += bins[b];
                    if (p02[i] == 0 && cumulative >= target02) p02[i] = b / 255.0;
                    if (p05[i] == 0 && cumulative >= target05) p05[i] = b / 255.0;
                    if (p50[i] == 0 && cumulative >= target50) p50[i] = b / 255.0;
                    if (p95[i] == 0 && cumulative >= target95) p95[i] = b / 255.0;
                    if (p98[i] == 0 && cumulative >= target98) p98[i] = b / 255.0;
                }
                if (p95[i] == 0) p95[i] = 1.0;
                if (p98[i] == 0) p98[i] = 1.0;
            }
            finally
            {
                foreach (var c in ch) c?.Dispose();
            }
        }

        m.P02_R = p02[2]; m.P02_G = p02[1]; m.P02_B = p02[0];
        m.P05_R = p05[2]; m.P05_G = p05[1]; m.P05_B = p05[0];
        m.P50_R = p50[2]; m.P50_G = p50[1]; m.P50_B = p50[0];
        m.P95_R = p95[2]; m.P95_G = p95[1]; m.P95_B = p95[0];
        m.P98_R = p98[2]; m.P98_G = p98[1]; m.P98_B = p98[0];
        return m;
    }
}
