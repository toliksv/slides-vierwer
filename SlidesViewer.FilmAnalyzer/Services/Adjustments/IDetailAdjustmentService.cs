using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services.Adjustments;

/// <summary>
/// Настройка параметров детализации: резкость, усиление краёв и микроконтраст.
/// </summary>
public interface IDetailAdjustmentService
{
    void Apply(CaptureOneAdjustments adjustments, FilmAnalysisResult result);
}
