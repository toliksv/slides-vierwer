using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlidesViewer.CorrectionWorker.Contracts.Analyze;
using SlidesViewer.CorrectionWorker.Contracts.Analyze.Mappers;
using SlidesViewer.CorrectionWorker.Host.Services;
using System.Text.Json;

namespace SlidesViewer.CorrectionWorker.Host;

public class KafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings;
    private readonly IFilmAnalysisService _analysisService;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KafkaConsumerService(
        IOptions<KafkaSettings> kafkaSettings,
        IFilmAnalysisService analysisService,
        ILogger<KafkaConsumerService> logger)
    {
        _kafkaSettings = kafkaSettings.Value;
        _analysisService = analysisService;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SessionTimeoutMs = 6000,
            MaxPollIntervalMs = 300000
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
            .SetErrorHandler((_, error) =>
            {
                _logger.LogWarning("Kafka error: {Error}", error);
            })
            .Build();

        consumer.Subscribe(_kafkaSettings.AnalysisTopic);

        _logger.LogInformation("Kafka consumer started. Listening on topic: {Topic}", _kafkaSettings.AnalysisTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult.Message == null)
                        continue;

                    var messageStr = consumeResult.Message.Value;

                    if (string.IsNullOrWhiteSpace(messageStr))
                    {
                        _logger.LogWarning("Received empty message from Kafka");
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    var avroMessage = JsonSerializer.Deserialize<SlideAnalysisRequestedMessage>(messageStr, _jsonOptions);

                    if (avroMessage == null || string.IsNullOrWhiteSpace(avroMessage.SlideFilePath))
                    {
                        _logger.LogWarning("Received invalid message from Kafka: {Message}", messageStr);
                        consumer.Commit(consumeResult);
                        continue;
                    }

                    var request = SlideAnalysisRequestedMessageMapper.Map(avroMessage);

                    _logger.LogInformation("Received analysis request for file: {FilePath}", request.SlideFilePath);

                    var jobId = await _analysisService.AnalyzeFileAsync(request.SlideFilePath);

                    if (jobId != null)
                    {
                        _logger.LogInformation("Analysis completed for {FilePath}, JobId: {JobId}", request.SlideFilePath, jobId);
                    }
                    else
                    {
                        _logger.LogError("Analysis failed for {FilePath}", request.SlideFilePath);
                    }

                    consumer.Commit(consumeResult);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka message");
                }
            }
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }
}
