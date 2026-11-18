using SlidesViewer.CorrectionWorker.Contracts.Analyze;

namespace SlidesViewer.CorrectionWorker.Contracts.Analyze.Mappers;

public static class SlideAnalysisRequestedMessageMapper
{
    public static SlideAnalysisRequested Map(SlideAnalysisRequestedMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (string.IsNullOrWhiteSpace(message.SlideFilePath))
            throw new ArgumentException("SlideFilePath cannot be null or empty", nameof(message));

        return new SlideAnalysisRequested(message.SlideFilePath);
    }
}
