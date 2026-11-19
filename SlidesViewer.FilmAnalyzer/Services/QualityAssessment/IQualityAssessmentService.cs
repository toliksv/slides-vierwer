namespace SlidesViewer.FilmAnalyzer.Services.QualityAssessment;

/// <summary>
/// Оценка качества отсканированного слайда.
/// </summary>
public interface IQualityAssessmentService
{
    Dtos.Analysis.QualityAssessment Assess(
        Dtos.Analysis.ColorDegradationAnalysis color,
        Dtos.Analysis.PhysicalDefectAnalysis physical);
}
