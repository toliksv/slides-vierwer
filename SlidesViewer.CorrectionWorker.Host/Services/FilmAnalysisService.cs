using OpenCvSharp;
using SlidesViewer.FilmAnalyzer;
using SlidesViewer.FilmAnalyzer.Dtos;
using SlidesViewer.FilmAnalyzer.Generators;

namespace SlidesViewer.CorrectionWorker.Host.Services;

public interface IFilmAnalysisService
{
    Task<string?> AnalyzeFileAsync(string filePath);
}

public class FilmAnalysisService : IFilmAnalysisService
{
    private readonly IFilmScannerAnalyzer _scannerAnalyzer;
    private readonly ICaptureOneStyleGenerator _styleGenerator;
    private readonly string _captureOneScriptsFolder;
    private readonly string _tempDirectory;

    public FilmAnalysisService(
        IFilmScannerAnalyzer scannerAnalyzer,
        ICaptureOneStyleGenerator styleGenerator,
        CaptureOneSettings captureOneSettings)
    {
        _scannerAnalyzer = scannerAnalyzer;
        _styleGenerator = styleGenerator;
        _captureOneScriptsFolder = captureOneSettings.ScriptsFolder;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SlidesViewer");
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task<string?> AnalyzeFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[FilmAnalysisService] File not found: {filePath}");
            return null;
        }

        try
        {
            Console.WriteLine($"[FilmAnalysisService] Starting analysis: {filePath}");

            // Perform analysis synchronously (CPU-intensive)
            var analysisResult = _scannerAnalyzer.Analyze(filePath);

            // Save Capture One style with folder structure
            SaveCaptureOneStyle(filePath, analysisResult);

            Console.WriteLine($"[FilmAnalysisService] Analysis completed: {filePath}");
            return Guid.NewGuid().ToString("N");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FilmAnalysisService] Error analyzing {filePath}:");
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    private void SaveCaptureOneStyle(string sourceFilePath, FilmAnalysisResult analysis)
    {
        // Source folder name becomes the subfolder name
        string sourceFolderName = Path.GetFileName(Path.GetDirectoryName(sourceFilePath) ?? string.Empty);
        if (string.IsNullOrEmpty(sourceFolderName))
            sourceFolderName = "default";

        // Create subfolder under CaptureOneScriptsFolder
        string scriptSubfolder = Path.Combine(_captureOneScriptsFolder, sourceFolderName);
        Directory.CreateDirectory(scriptSubfolder);

        // Style file name = TIFF file name without extension
        string styleFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        string stylePath = Path.Combine(scriptSubfolder, $"{styleFileName}.costyle");

        // Generate and save style
        string style = _styleGenerator.GenerateStyle(analysis);
        File.WriteAllText(stylePath, style);

        Console.WriteLine($"[FilmAnalysisService] Style saved: {stylePath}");
    }
}
