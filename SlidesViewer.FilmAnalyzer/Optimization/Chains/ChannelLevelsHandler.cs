using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Настройка поканальных уровней (чёрный / белый) на основе метрик кадра.
/// </summary>
public class ChannelLevelsHandler : OptimizationHandlerBase
{
    public override int Order => 1;

    protected override void Process(OptimizationContext context)
    {
        var m = context.Metrics;
        bool need = context.Iteration == 1 || m.ClipHiR > 0.001 || m.ClipHiG > 0.001 || m.ClipHiB > 0.001;
        if (!need) return;

        var adj = context.Adjustments.Exposure;
        adj.ShadowR = Math.Clamp(m.P01_R * 0.9, 0.01, 0.25);
        adj.ShadowG = Math.Clamp(m.P01_G * 0.9, 0.01, 0.25);
        adj.ShadowB = Math.Clamp(m.P01_B * 0.9, 0.01, 0.25);

        adj.HighlightR = Math.Clamp(m.P99_R * 1.02, 0.88, 1.00);
        adj.HighlightG = Math.Clamp(m.P99_G * 1.02, 0.88, 1.00);
        adj.HighlightB = Math.Clamp(m.P99_B * 1.02, 0.88, 1.00);

        context.ShadowAdjusted = true;
        context.RecordChange("Levels",
            $"S=[{adj.ShadowR:F2},{adj.ShadowG:F2},{adj.ShadowB:F2}] H=[{adj.HighlightR:F2},{adj.HighlightG:F2},{adj.HighlightB:F2}]");
    }
}
