namespace SlidesViewer.Host.Services;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string AnalysisTopic { get; set; } = string.Empty;
}
