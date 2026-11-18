using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlidesViewer.CorrectionWorker.Host;
using SlidesViewer.CorrectionWorker.Host.Services;
using SlidesViewer.FilmAnalyzer;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register settings
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection(KafkaSettings.SectionName));
builder.Services.Configure<CaptureOneSettings>(builder.Configuration.GetSection(CaptureOneSettings.SectionName));

// Bind settings to concrete instances for direct injection
var captureOneSection = builder.Configuration.GetSection(CaptureOneSettings.SectionName);
var captureOneSettings = new CaptureOneSettings
{
    ScriptsFolder = captureOneSection["ScriptsFolder"] ?? "E:\\CaptureOneScripts"
};
builder.Services.AddSingleton(captureOneSettings);

// Film Analyzer services
builder.Services.AddSingleton<IColorDegradationAnalyzer, ColorDegradationAnalyzer>();
builder.Services.AddSingleton<IPhysicalDefectAnalyzer, PhysicalDefectAnalyzer>();
builder.Services.AddSingleton<IQualityAssessmentService, QualityAssessmentService>();
builder.Services.AddSingleton<ICaptureOneStyleGenerator, CaptureOneStyleGenerator>();
builder.Services.AddSingleton<IFilmScannerAnalyzer, FilmScannerAnalyzer>();

// Correction Worker services
builder.Services.AddSingleton<IFilmAnalysisService, FilmAnalysisService>();

// Background service
builder.Services.AddHostedService<KafkaConsumerService>();

var host = builder.Build();
host.Run();
