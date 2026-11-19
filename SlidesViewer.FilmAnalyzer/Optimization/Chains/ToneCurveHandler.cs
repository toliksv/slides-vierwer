using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Задание базовой тоновой кривой и мягкая правка средних тонов каналов.
/// </summary>
public class ToneCurveHandler : OptimizationHandlerBase
{
    public override int Order => 4;

    protected override void Process(OptimizationContext context)
    {
        var adj = context.Adjustments.ToneCurve;

        if (context.Iteration == 1)
        {
            int[] points = context.IsNegative
                ? new[] { 0, 0, 16384, 14000, 32768, 32768, 49152, 51500, 65535, 65535 }
                : new[] { 0, 0, 16384, 15800, 32768, 32768, 49152, 49700, 65535, 65535 };
            adj.LuminancePoints = points;
        }

        if (context.WhiteBalanceAdjusted)
            return;

        var m = context.Metrics;
        bool hasClip = m.ClipHiR > 0.001 || m.ClipHiG > 0.001 || m.ClipHiB > 0.001;
        if (hasClip)
            return;

        var rp = adj.RedPoints;
        var gp = adj.GreenPoints;
        var bp = adj.BluePoints;
        int mid = Math.Max(1, Math.Min(rp.Length - 1, Math.Min(gp.Length - 1, bp.Length - 1)));

        rp[mid] = Math.Clamp(rp[mid] + 500, 0, 65535);
        gp[mid] = Math.Clamp(gp[mid] - 500, 0, 65535);
        bp[mid] = Math.Clamp(bp[mid] + 500, 0, 65535);

        context.RecordChange("ToneCurve", "soft midtone channel ±500");
    }
}
