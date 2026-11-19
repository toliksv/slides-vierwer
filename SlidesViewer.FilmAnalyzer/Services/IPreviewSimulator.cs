using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;

namespace SlidesViewer.FilmAnalyzer.Services;

/// <summary>
/// Симуляция превью — применяет коррекции к изображению для виртуального отображения результата.
/// </summary>
public interface IPreviewSimulator
{
    /// <summary>
    /// Симулирует превью, применяя коррекции к исходному изображению.
    /// </summary>
    Mat Simulate(Mat source, CaptureOneAdjustments adjustments);
}
