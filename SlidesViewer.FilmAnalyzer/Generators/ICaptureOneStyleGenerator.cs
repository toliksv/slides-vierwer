using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Generators;

public interface ICaptureOneStyleGenerator
{
    string GenerateStyle(FilmAnalysisResult analysis);
}
