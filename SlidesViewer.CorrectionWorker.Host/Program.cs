using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SlidesViewer.CorrectionWorker.Host;
using SlidesViewer.CorrectionWorker.Host.Services;
using SlidesViewer.FilmAnalyzer;
using SlidesViewer.FilmAnalyzer.Analyzers;
using SlidesViewer.FilmAnalyzer.Generators;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;
using SlidesViewer.FilmAnalyzer.Services;

var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Extensions", LogEventLevel.Warning)
    .MinimumLevel.Override("Confluent", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "Logs/slides-correction-worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = logger;

try
{
    Log.Information("Starting slides correction worker host");

    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog((sp, cfg) => cfg
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Extensions", LogEventLevel.Warning)
        .MinimumLevel.Override("Confluent", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            "Logs/slides-correction-worker-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

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

    // Optimization chain handlers (registered as IOptimizationHandler for collection injection)
    builder.Services.AddSingleton<IOptimizationHandler, SlidesViewer.FilmAnalyzer.Optimization.Chains.ChannelLevelsHandler>();
    builder.Services.AddSingleton<IOptimizationHandler, SlidesViewer.FilmAnalyzer.Optimization.Chains.ExposureGuardHandler>();
    builder.Services.AddSingleton<IOptimizationHandler, SlidesViewer.FilmAnalyzer.Optimization.Chains.WhiteBalanceHandler>();
    builder.Services.AddSingleton<IOptimizationHandler, SlidesViewer.FilmAnalyzer.Optimization.Chains.ToneCurveHandler>();
    builder.Services.AddSingleton<IOptimizationChainFactory, OptimizationChainFactory>();
    builder.Services.AddSingleton<IIterativeCorrectionOptimizer, IterativeCorrectionOptimizer>();
    builder.Services.AddSingleton<IFilmScannerAnalyzer, FilmScannerAnalyzer>();

    // Correction Worker services
    builder.Services.AddSingleton<IFilmAnalysisService, FilmAnalysisService>();

    // Background service
    builder.Services.AddHostedService<KafkaConsumerService>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down");
    Log.CloseAndFlush();
}
