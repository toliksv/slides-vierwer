using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Плавная коррекция баланса белого по сдвигу средних тонов.
/// </summary>
public class WhiteBalanceHandler : OptimizationHandlerBase
{
    public override int Order => 3;

    protected override void Process(OptimizationContext context)
    {
        var m = context.Metrics;
        var adj = context.Adjustments.WhiteBalance;

        double blueRedDiff = m.P50_B - m.P50_R;
        double greenCast = m.P50_G - (m.P50_R + m.P50_B) / 2.0;

        if (Math.Abs(blueRedDiff) <= 0.03 && Math.Abs(greenCast) <= 0.03)
            return;

        if (Math.Abs(blueRedDiff) > 0.03)
        {
            double step = blueRedDiff > 0 ? 150.0 : -150.0;
            adj.ColorTemperature = Math.Clamp(adj.ColorTemperature + step, 5000, 6000);
        }

        if (Math.Abs(greenCast) > 0.03)
        {
            double step = greenCast > 0 ? 1.0 : -1.0;
            adj.Tint = Math.Clamp(adj.Tint + step, -5, 5);
        }

        context.WhiteBalanceAdjusted = true;
        context.RecordChange("WhiteBalance", $"CT={adj.ColorTemperature:F0}K Tint={adj.Tint:F2}");
    }
}
