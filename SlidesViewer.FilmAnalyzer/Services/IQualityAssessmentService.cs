using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services;

/// <summary>
/// Оценка качества отсканированного слайда.
/// </summary>
public interface IQualityAssessmentService
{
    QualityAssessment Assess(ColorDegradationAnalysis color, PhysicalDefectAnalysis physical);
}
