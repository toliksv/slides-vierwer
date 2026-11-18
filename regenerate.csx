using Microsoft.Extensions.DependencyInjection;
using SlidesViewer.FilmAnalyzer;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Services;

var sourceFile = @"E:\ИИ слайды\Scripts\В гостях 83-85_\Нальчик_п2_18.tif";
var outputDir = @"E:\ИИ слайды\Scripts\ИИ слайды";

if (!File.Exists(sourceFile))
{
    Console.WriteLine($"File not found: {sourceFile}");
    return;
}

var services = new ServiceCollection();
services.AddSingleton<IColorDegradationAnalyzer, ColorDegradationAnalyzer>();
services.AddSingleton<IPhysicalDefectAnalyzer, PhysicalDefectAnalyzer>();
services.AddSingleton<IQualityAssessmentService, QualityAssessmentService>();
services.AddSingleton<ICaptureOneStyleGenerator, CaptureOneStyleGenerator>();
services.AddSingleton<IFilmScannerAnalyzer, FilmScannerAnalyzer>();

var sp = services.BuildServiceProvider();
var analyzer = sp.GetRequiredService<IFilmScannerAnalyzer>();
var generator = sp.GetRequiredService<ICaptureOneStyleGenerator>();

Console.WriteLine("Analyzing...");
var result = analyzer.Analyze(sourceFile);

Console.WriteLine($"R_Mean={result.Color.R_Mean:F0} G_Mean={result.Color.G_Mean:F0} B_Mean={result.Color.B_Mean:F0}");
Console.WriteLine($"MidtoneRedGain={result.Corrections.ColorCalibration.MidtoneRedGain:F3}");
Console.WriteLine($"MidtoneBlueGain={result.Corrections.ColorCalibration.MidtoneBlueGain:F3}");
Console.WriteLine($"ShadowTint={result.Corrections.ColorCalibration.ShadowTint:F3}");
Console.WriteLine($"BlackLevelRed={result.Corrections.Exposure.BlackLevelRed}");

// Check tone curve midpoints
var lum = result.Corrections.ToneCurve.LuminancePoints;
if (lum.Length >= 6)
{
    Console.WriteLine($"Lum mid Y={lum[5] / 65535.0:F3}");
}
var red = result.Corrections.ToneCurve.RedPoints;
if (red.Length >= 6)
{
    Console.WriteLine($"Red mid Y={red[3] / 65535.0:F3}");
}
var green = result.Corrections.ToneCurve.GreenPoints;
if (green.Length >= 6)
{
    Console.WriteLine($"Green mid Y={green[3] / 65535.0:F3}");
}
var blue = result.Corrections.ToneCurve.BluePoints;
if (blue.Length >= 6)
{
    Console.WriteLine($"Blue mid Y={blue[3] / 65535.0:F3}");
}

var style = generator.GenerateStyle(result);
var outputPath = Path.Combine(outputDir, "Нальчик_п2_18.costyle");
File.WriteAllText(outputPath, style);
Console.WriteLine($"Style saved: {outputPath}");
