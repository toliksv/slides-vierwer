using SlidesViewer.FilmAnalyzer.Dtos;

namespace SlidesViewer.FilmAnalyzer.Generators;

public interface ICaptureOneStyleGenerator
{
    string GenerateStyle(FilmAnalysisResult analysis);
}
