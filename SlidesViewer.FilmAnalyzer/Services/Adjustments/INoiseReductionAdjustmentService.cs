using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services.Adjustments;

/// <summary>
/// Настройка параметров шумоподавления: яркостной и цветовой шум, восстановление деталей.
/// </summary>
public interface INoiseReductionAdjustmentService
{
    void Apply(CaptureOneAdjustments adjustments, FilmAnalysisResult result);
}
