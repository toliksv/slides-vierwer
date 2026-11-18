using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Optimization;

/// <summary>
/// Метрики текущего кадра, используемые шагами оптимизации.
/// </summary>
public class FrameMetrics
{
    public double ClipHiR { get; set; }
    public double ClipHiG { get; set; }
    public double ClipHiB { get; set; }

    public double ClipLoR { get; set; }
    public double ClipLoG { get; set; }
    public double ClipLoB { get; set; }

    public double P50_R { get; set; }
    public double P50_G { get; set; }
    public double P50_B { get; set; }

    public double P95_R { get; set; }
    public double P95_G { get; set; }
    public double P95_B { get; set; }

    public bool IsNegative { get; set; }
}

/// <summary>
/// Шаг пайплайна оптимизации коррекций.
/// </summary>
public interface IOptimizationStep
{
    bool ApplyStep(CaptureOneAdjustments adjustments, FrameMetrics metrics, out string logMessage);
}
