using System.Text.Json.Serialization;

namespace SlidesViewer.FilmAnalyzer.Dtos;

public class HistogramData
{
    [JsonPropertyOrder(0)]
    public int[] Values { get; set; } = Array.Empty<int>();

    [JsonPropertyOrder(1)]
    public int Bins { get; set; }

    [JsonPropertyOrder(2)]
    public double MinValue { get; set; }

    [JsonPropertyOrder(3)]
    public double MaxValue { get; set; }

    [JsonPropertyOrder(4)]
    public double Mean { get; set; }

    [JsonPropertyOrder(5)]
    public double StdDev { get; set; }

    public HistogramData()
    {
    }

    public HistogramData(int[] values, int bins, double min, double max, double mean, double stdDev)
    {
        Values = values;
        Bins = bins;
        MinValue = min;
        MaxValue = max;
        Mean = mean;
        StdDev = stdDev;
    }
}
