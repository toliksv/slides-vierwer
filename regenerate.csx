#r "nuget: Microsoft.Extensions.DependencyInjection, 8.0.0"
#r "nuget: OpenCvSharp4, 4.10.0.20240615"
#r "nuget: OpenCvSharp4.Extensions, 4.10.0.20240615"
#r "nuget: SixLabors.ImageSharp, 3.1.6"
#r "SlidesViewer.FilmAnalyzer/bin/Debug/net8.0/SlidesViewer.FilmAnalyzer.dll"
using Microsoft.Extensions.DependencyInjection;
using SlidesViewer.FilmAnalyzer;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;
using SlidesViewer.FilmAnalyzer.Services.Adjustments;
using SlidesViewer.FilmAnalyzer.Services.QualityAssessment;

var sourceFile = @"E:\ИИ слайды\Нальчик_п2_18.tiff";
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

// Optimization chain handlers
services.AddSingleton<IOptimizationHandler, ChannelLevelsHandler>();
services.AddSingleton<IOptimizationHandler, ExposureGuardHandler>();
services.AddSingleton<IOptimizationHandler, WhiteBalanceHandler>();
services.AddSingleton<IOptimizationHandler, ToneCurveHandler>();
services.AddSingleton<IOptimizationChainFactory, OptimizationChainFactory>();
services.AddSingleton<ExposureOptimizationStep>();
services.AddSingleton<WhiteBalanceOptimizationStep>();
services.AddSingleton<ToneCurveOptimizationStep>();
services.AddSingleton<IIterativeCorrectionOptimizer, IterativeCorrectionOptimizer>();
services.AddSingleton<IFilmScannerAnalyzer, FilmScannerAnalyzer>();

var sp = services.BuildServiceProvider();
var analyzer = sp.GetRequiredService<IFilmScannerAnalyzer>();
var generator = sp.GetRequiredService<ICaptureOneStyleGenerator>();

Console.WriteLine("Analyzing...");
var result = analyzer.Analyze(sourceFile);

Console.WriteLine("\n--- Optimization Log ---");
foreach (var line in result.OptimizationLog)
    Console.WriteLine(line);

Console.WriteLine("\n--- Final Adjustments ---");
Console.WriteLine($"Exposure={result.Corrections.Exposure.Exposure:F2}");
Console.WriteLine($"WhiteBalanceTemperature={result.Corrections.WhiteBalance.ColorTemperature:F0}");
Console.WriteLine($"WhiteBalanceTint={result.Corrections.WhiteBalance.Tint:F2}");
var redPts = result.Corrections.ToneCurve.RedPoints;
var greenPts = result.Corrections.ToneCurve.GreenPoints;
var bluePts = result.Corrections.ToneCurve.BluePoints;
var redMid = redPts.Length >= 4 ? redPts[3] / 65535.0 : -1;
var greenMid = greenPts.Length >= 4 ? greenPts[3] / 65535.0 : -1;
var blueMid = bluePts.Length >= 4 ? bluePts[3] / 65535.0 : -1;
Console.WriteLine($"RedMid={redMid:F3}");
Console.WriteLine($"GreenMid={greenMid:F3}");
Console.WriteLine($"BlueMid={blueMid:F3}");

var style = generator.GenerateStyle(result);
var outputPath = Path.Combine(outputDir, "Нальчик_п2_18.costyle");
File.WriteAllText(outputPath, style);
Console.WriteLine($"Style saved: {outputPath}");
