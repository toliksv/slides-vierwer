using System.Text.Json.Serialization;
using SlidesViewer.FilmAnalyzer.Dtos.Common;

namespace SlidesViewer.FilmAnalyzer.Dtos.Analysis;

/// <summary>
/// Анализ деградации цвета: статистика каналов, цветовой оттенок, затухание, гистограммы, оценка белого баланса и тип плёнки.
/// </summary>
public class ColorDegradationAnalysis
{
    [JsonPropertyOrder(0)]
    public double R_Mean { get; set; }

    [JsonPropertyOrder(1)]
    public double G_Mean { get; set; }

    [JsonPropertyOrder(2)]
    public double B_Mean { get; set; }

    [JsonPropertyOrder(3)]
    public double R_StdDev { get; set; }

    [JsonPropertyOrder(4)]
    public double G_StdDev { get; set; }

    [JsonPropertyOrder(5)]
    public double B_StdDev { get; set; }

    [JsonPropertyOrder(6)]
    public ColorCastType Cast { get; set; }

    [JsonPropertyOrder(7)]
    public double CastIntensity { get; set; }

    [JsonPropertyOrder(8)]
    public double R_Fading { get; set; }

    [JsonPropertyOrder(9)]
    public double G_Fading { get; set; }

    [JsonPropertyOrder(10)]
    public double B_Fading { get; set; }

    [JsonPropertyOrder(11)]
    public double OverallFading { get; set; }

    [JsonPropertyOrder(12)]
    public double OverallSaturation { get; set; }

    [JsonPropertyOrder(13)]
    public HistogramData R_Histogram { get; set; } = new();

    [JsonPropertyOrder(14)]
    public HistogramData G_Histogram { get; set; } = new();

    [JsonPropertyOrder(15)]
    public HistogramData B_Histogram { get; set; } = new();

    [JsonPropertyOrder(16)]
    public double EstimatedWhiteBalance_R { get; set; }

    [JsonPropertyOrder(17)]
    public double EstimatedWhiteBalance_B { get; set; }

    [JsonPropertyOrder(18)]
    public double SuggestedWhiteBalance_R { get; set; }

    [JsonPropertyOrder(19)]
    public double SuggestedWhiteBalance_B { get; set; }

    // Percentiles for each channel (16-bit 0-65535)
    [JsonPropertyOrder(20)]
    public double R_P02 { get; set; }

    [JsonPropertyOrder(21)]
    public double R_P25 { get; set; }

    [JsonPropertyOrder(22)]
    public double R_P50 { get; set; }

    [JsonPropertyOrder(23)]
    public double R_P75 { get; set; }

    [JsonPropertyOrder(24)]
    public double R_P98 { get; set; }

    [JsonPropertyOrder(25)]
    public double G_P02 { get; set; }

    [JsonPropertyOrder(26)]
    public double G_P25 { get; set; }

    [JsonPropertyOrder(27)]
    public double G_P50 { get; set; }

    [JsonPropertyOrder(28)]
    public double G_P75 { get; set; }

    [JsonPropertyOrder(29)]
    public double G_P98 { get; set; }

    [JsonPropertyOrder(30)]
    public double B_P02 { get; set; }

    [JsonPropertyOrder(31)]
    public double B_P25 { get; set; }

    [JsonPropertyOrder(32)]
    public double B_P50 { get; set; }

    [JsonPropertyOrder(33)]
    public double B_P75 { get; set; }

    [JsonPropertyOrder(34)]
    public double B_P98 { get; set; }

    // Optical density D = -Log10(val / 65535) for shadows and highlights
    [JsonPropertyOrder(35)]
    public double R_DensityShadow { get; set; }

    [JsonPropertyOrder(36)]
    public double R_DensityHighlight { get; set; }

    [JsonPropertyOrder(37)]
    public double G_DensityShadow { get; set; }

    [JsonPropertyOrder(38)]
    public double G_DensityHighlight { get; set; }

    [JsonPropertyOrder(39)]
    public double B_DensityShadow { get; set; }

    [JsonPropertyOrder(40)]
    public double B_DensityHighlight { get; set; }

    // Auto-detected film medium type
    [JsonPropertyOrder(41)]
    public FilmMediumType MediumType { get; set; }

    public ColorDegradationAnalysis()
    {
    }
}
