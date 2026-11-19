using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Корректировка экспозиции при клиппинге светов / теней.
/// </summary>
public class ExposureGuardHandler : OptimizationHandlerBase
{
    public override int Order => 2;

    protected override void Process(OptimizationContext context)
    {
        var m = context.Metrics;
        var adj = context.Adjustments.Exposure;
        double maxClipHi = Math.Max(m.ClipHiR, Math.Max(m.ClipHiG, m.ClipHiB));

        if (maxClipHi > 0.005)
        {
            adj.Exposure = Math.Round(adj.Exposure - 0.10, 2);
            context.RecordChange("Exposure", $"-0.10 EV (clipHi={maxClipHi:P1})");
        }

        if (!context.ShadowAdjusted)
        {
            double maxClipLo = Math.Max(m.ClipLoR, Math.Max(m.ClipLoG, m.ClipLoB));
            if (maxClipLo > 0.05)
            {
                adj.Exposure = Math.Round(adj.Exposure + 0.10, 2);
                context.RecordChange("Exposure", $"+0.10 EV (clipLo={maxClipLo:P1})");
            }
        }
    }
}
