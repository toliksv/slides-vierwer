using SlidesViewer.CorrectionWorker.Contracts.Analyze;

namespace SlidesViewer.Host.Services;

public interface IAnalysisMessagePublisher
{
    Task PublishAsync(SlideAnalysisRequested request, CancellationToken cancellationToken = default);
}
