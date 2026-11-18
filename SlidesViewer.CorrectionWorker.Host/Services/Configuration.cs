namespace SlidesViewer.CorrectionWorker.Host;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string ConsumerGroupId { get; set; } = string.Empty;
    public string AnalysisTopic { get; set; } = string.Empty;
}

public class CaptureOneSettings
{
    public const string SectionName = "CaptureOne";

    public string ScriptsFolder { get; set; } = string.Empty;
}
