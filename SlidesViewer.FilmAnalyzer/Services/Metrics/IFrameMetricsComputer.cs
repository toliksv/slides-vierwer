using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;

namespace SlidesViewer.FilmAnalyzer.Services.Metrics;

/// <summary>
/// Вычисляет метрики кадра (клиппинг, перцентили по каналам) из изображения.
/// </summary>
public interface IFrameMetricsComputer
{
    /// <summary>
    /// Вычисляет метрики кадра из нормализованного 32FC3 изображения (значения 0..1).
    /// </summary>
    FrameMetrics Compute(Mat normalizedImage, bool isNegative);
}
