namespace SlidesViewer.CorrectionWorker.Contracts.Analyze;

public sealed class SlideAnalysisRequestedMessage
{
    public string? SlideFilePath { get; set; }
}
