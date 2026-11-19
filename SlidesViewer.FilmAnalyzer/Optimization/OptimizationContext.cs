using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;

namespace SlidesViewer.FilmAnalyzer.Optimization;

/// <summary>
/// Контекст, передаваемый по цепочке шагов оптимизации.
/// </summary>
public sealed class OptimizationContext
{
    public CaptureOneAdjustments Adjustments { get; init; } = null!;
    public FrameMetrics Metrics { get; set; } = new();
    public bool IsNegative { get; init; }
    public int Iteration { get; set; }
    public bool ShadowAdjusted { get; set; }
    public bool WhiteBalanceAdjusted { get; set; }
    public HashSet<string> ModifiedKeys { get; } = new();
    public List<string> Log { get; } = new();

    public void RecordChange(string key, string message)
    {
        ModifiedKeys.Add(key);
        Log.Add($"Iter {Iteration} [{key}]: {message}");
    }
}
