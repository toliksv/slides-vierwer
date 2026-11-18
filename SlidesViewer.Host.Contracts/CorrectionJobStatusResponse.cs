using System.Text.Json.Serialization;

namespace SlidesViewer.Host.Contracts;

public enum CorrectionJobStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class CorrectionJobStatusResponse
{
    [JsonPropertyOrder(0)]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    public CorrectionJobStatus Status { get; set; }

    [JsonPropertyOrder(2)]
    public int Progress { get; set; }

    [JsonPropertyOrder(3)]
    public string? ErrorMessage { get; set; }

    public CorrectionJobStatusResponse()
    {
    }
}
