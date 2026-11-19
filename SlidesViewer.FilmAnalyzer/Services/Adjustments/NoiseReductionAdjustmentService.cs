using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;

namespace SlidesViewer.FilmAnalyzer.Services.Adjustments;

public class NoiseReductionAdjustmentService : INoiseReductionAdjustmentService
{
    public void Apply(CaptureOneAdjustments adjustments, FilmAnalysisResult result)
    {
        // Noise estimation based on std dev in uniform regions
        double avgStdDev = (result.Color.R_StdDev + result.Color.G_StdDev + result.Color.B_StdDev) / 3.0;

        // Higher std dev in midtones suggests more noise
        double noiseLevel = avgStdDev / 10000.0; // Normalize

        adjustments.NoiseReduction.Luminance = Math.Min(50, noiseLevel * 30);
        adjustments.NoiseReduction.Color = Math.Min(50, noiseLevel * 20);
        adjustments.NoiseReduction.LuminanceDetail = Math.Max(0, 50 - adjustments.NoiseReduction.Luminance);
        adjustments.NoiseReduction.ColorDetail = Math.Max(0, 50 - adjustments.NoiseReduction.Color);
    }
}
