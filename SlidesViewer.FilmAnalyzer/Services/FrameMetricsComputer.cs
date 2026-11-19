using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;

namespace SlidesViewer.FilmAnalyzer.Services;

/// <summary>
/// Вычисляет метрики кадра (клиппинг, перцентили по каналам) из изображения.
/// </summary>
public class FrameMetricsComputer : IFrameMetricsComputer
{
    public FrameMetrics Compute(Mat normalizedImage, bool isNegative)
    {
        var m = new FrameMetrics { IsNegative = isNegative };
        int totalPixels = normalizedImage.Rows * normalizedImage.Cols;

        double[] p01 = new double[3], p02 = new double[3], p05 = new double[3];
        double[] p50 = new double[3], p95 = new double[3], p98 = new double[3], p99 = new double[3];

        for (int i = 0; i < 3; i++)
        {
            Mat[] ch = null!;
            try
            {
                Cv2.Split(normalizedImage, out ch);
                using var single = ch[i];

                using var maskHiLocal = new Mat();
                Cv2.Compare(single, new Scalar(0.99), maskHiLocal, CmpType.GT);
                double clipHi = Cv2.CountNonZero(maskHiLocal) / (double)totalPixels;

                using var maskLoLocal = new Mat();
                Cv2.Compare(single, new Scalar(0.005), maskLoLocal, CmpType.LT);
                double clipLo = Cv2.CountNonZero(maskLoLocal) / (double)totalPixels;

                switch (i)
                {
                    case 0: m.ClipHiB = clipHi; m.ClipLoB = clipLo; break;
                    case 1: m.ClipHiG = clipHi; m.ClipLoG = clipLo; break;
                    case 2: m.ClipHiR = clipHi; m.ClipLoR = clipLo; break;
                }

                float[] range = { 0f, 1f };
                int[] histSize = { 256 };
                using var hist = new Mat();
                Cv2.CalcHist(new[] { single }, new[] { 0 }, null, hist, 1, histSize, new[] { range });
                hist.GetArray<float>(out var bins);
                double cumulative = 0;
                double target01 = totalPixels * 0.01;
                double target02 = totalPixels * 0.02;
                double target05 = totalPixels * 0.05;
                double target50 = totalPixels * 0.50;
                double target95 = totalPixels * 0.95;
                double target98 = totalPixels * 0.98;
                double target99 = totalPixels * 0.99;
                for (int b = 0; b < 256; b++)
                {
                    cumulative += bins[b];
                    if (p01[i] == 0 && cumulative >= target01) p01[i] = b / 255.0;
                    if (p02[i] == 0 && cumulative >= target02) p02[i] = b / 255.0;
                    if (p05[i] == 0 && cumulative >= target05) p05[i] = b / 255.0;
                    if (p50[i] == 0 && cumulative >= target50) p50[i] = b / 255.0;
                    if (p95[i] == 0 && cumulative >= target95) p95[i] = b / 255.0;
                    if (p98[i] == 0 && cumulative >= target98) p98[i] = b / 255.0;
                    if (p99[i] == 0 && cumulative >= target99) p99[i] = b / 255.0;
                }
                if (p95[i] == 0) p95[i] = 1.0;
                if (p98[i] == 0) p98[i] = 1.0;
                if (p99[i] == 0) p99[i] = 1.0;
            }
            finally
            {
                foreach (var c in ch) c?.Dispose();
            }
        }

        m.P01_R = p01[2]; m.P01_G = p01[1]; m.P01_B = p01[0];
        m.P02_R = p02[2]; m.P02_G = p02[1]; m.P02_B = p02[0];
        m.P05_R = p05[2]; m.P05_G = p05[1]; m.P05_B = p05[0];
        m.P50_R = p50[2]; m.P50_G = p50[1]; m.P50_B = p50[0];
        m.P95_R = p95[2]; m.P95_G = p95[1]; m.P95_B = p95[0];
        m.P98_R = p98[2]; m.P98_G = p98[1]; m.P98_B = p98[0];
        m.P99_R = p99[2]; m.P99_G = p99[1]; m.P99_B = p99[0];
        return m;
    }
}
