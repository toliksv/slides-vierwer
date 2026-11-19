using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;
using SlidesViewer.FilmAnalyzer.Utilities;

namespace SlidesViewer.FilmAnalyzer.Services;

public class IterativeCorrectionOptimizer : IIterativeCorrectionOptimizer
{
    private readonly IOptimizationChainFactory _chainFactory;
    private readonly IPreviewSimulator _previewSimulator;
    private readonly IFrameMetricsComputer _metricsComputer;

    public IterativeCorrectionOptimizer(
        IOptimizationChainFactory chainFactory,
        IPreviewSimulator previewSimulator,
        IFrameMetricsComputer metricsComputer)
    {
        _chainFactory = chainFactory;
        _previewSimulator = previewSimulator;
        _metricsComputer = metricsComputer;
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

        var chain = _chainFactory.Create();

        var context = new OptimizationContext { Adjustments = adj, IsNegative = isNegative };

        for (int iter = 1; iter <= 5; iter++)
        {
            context.Iteration = iter;
            var sim = _previewSimulator.Simulate(small, adj);
            context.Metrics = _metricsComputer.Compute(sim, isNegative);

            int prevCount = context.Log.Count;
            chain.Handle(context);

            if (context.Log.Count == prevCount)
            {
                context.Log.Add($"Iter {iter}: Converged");
                break;
            }
        }

        log = context.Log;
        return context.Adjustments;
    }
}
