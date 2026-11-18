using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlidesViewer.CorrectionWorker.Contracts.Analyze;
using System.Text.Json;

namespace SlidesViewer.Host.Services;

public sealed class AnalysisMessagePublisher : IAnalysisMessagePublisher, IDisposable
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<AnalysisMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private IProducer<Null, string>? _producer;

    public AnalysisMessagePublisher(
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<AnalysisMessagePublisher> logger)
    {
        _settings = kafkaSettings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogWarning("Kafka producer error: {Error}", error);
            })
            .Build();
    }

    public async Task PublishAsync(SlideAnalysisRequested request, CancellationToken cancellationToken = default)
    {
        var message = new SlideAnalysisRequestedMessage
        {
            SlideFilePath = request.SlideFilePath
        };

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var messageValue = new Message<Null, string> { Value = json };

        var produceTask = _producer!.ProduceAsync(_settings.AnalysisTopic, messageValue, cancellationToken);
        await produceTask.ConfigureAwait(false);

        _logger.LogInformation(
            "Analysis message published to Kafka topic '{Topic}' for file: {FilePath}",
            _settings.AnalysisTopic, request.SlideFilePath);
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
    }
}
