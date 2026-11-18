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
            ? [new ExposureOptimizationStep(), new WhiteBalanceOptimizationStep(), new ToneCurveOptimizationStep()]
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
        double rMul = adj.WhiteBalance.ColorTemperature / 5500.0;
        double bMul = 5500.0 / adj.WhiteBalance.ColorTemperature;
        Cv2.Split(img, out var channels);
        for (int c = 0; c < 3; c++)
        {
            channels[c].GetArray<float>(out var arr);
            for (int i = 0; i < arr.Length; i++)
            {
                double v = arr[i] * expMul;
                if (c == 0) v *= rMul;
                else if (c == 2) v *= bMul;
                arr[i] = (float)Math.Clamp((v - 0.05) / 0.9, 0.0, 1.0);
            }
        }
        var outMat = new Mat();
        Cv2.Merge(channels, outMat);
        return outMat;
    }

    private static FrameMetrics ComputeMetrics(Mat mat)
    {
        var m = new FrameMetrics();
        int totalPixels = mat.Rows * mat.Cols;
        double[] histBins = new double[3];
        double[] p50 = new double[3], p95 = new double[3];

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

                switch (i)
                {
                    case 0: m.ClipHiR = clipHi; m.ClipLoR = clipLo; break;
                    case 1: m.ClipHiG = clipHi; m.ClipLoG = clipLo; break;
                    case 2: m.ClipHiB = clipHi; m.ClipLoB = clipLo; break;
                }

                float[] range = { 0f, 1f };
                int[] histSize = { 256 };
                var hist = new Mat();
                Cv2.CalcHist(new[] { single }, new[] { 0 }, null, hist, 1, histSize, new[] { range });
                hist.GetArray<float>(out var bins);
                double cumulative = 0;
                double target50 = totalPixels * 0.50;
                double target95 = totalPixels * 0.95;
                for (int b = 0; b < 256; b++)
                {
                    cumulative += bins[b];
                    if (p50[i] == 0 && cumulative >= target50) p50[i] = b / 255.0;
                    if (cumulative >= target95) { p95[i] = b / 255.0; break; }
                }
                if (p95[i] == 0) p95[i] = 1.0;
            }
            finally
            {
                foreach (var c in ch) c?.Dispose();
            }
        }

        m.P50_R = p50[0]; m.P50_G = p50[1]; m.P50_B = p50[2];
        m.P95_R = p95[0]; m.P95_G = p95[1]; m.P95_B = p95[2];
        return m;
    }
}
