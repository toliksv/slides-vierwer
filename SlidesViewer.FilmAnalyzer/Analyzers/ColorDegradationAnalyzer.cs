using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Dtos.Common;
using SlidesViewer.FilmAnalyzer.Utilities;

namespace SlidesViewer.FilmAnalyzer.Analyzers;

public interface IColorDegradationAnalyzer
{
    ColorDegradationAnalysis Analyze(string filePath);
}

public class ColorDegradationAnalyzer : IColorDegradationAnalyzer
{
    private const int BitDepthMax = 65535;
    private const double IdealMidpoint = 32768.0;

    public ColorDegradationAnalysis Analyze(string filePath)
    {
        using var rawImage = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);
        
        if (rawImage.Empty())
            throw new InvalidOperationException($"Cannot read image: {filePath}");

        // Normalize to 16-bit 3-channel (handles CV_16UC4, CV_16UC1, CV_8UC*, etc.)
        using var image = ImageNormalizationUtility.NormalizeTo16BitColor(rawImage);

        // Crop 8% from each edge to exclude black scan frame before splitting
        int crop = (int)(Math.Min(image.Rows, image.Cols) * 0.08);
        using var roi = image[new Rect(crop, crop, image.Cols - 2 * crop, image.Rows - 2 * crop)].Clone();

        var analysis = new ColorDegradationAnalysis();

        // Split into BGR channels (OpenCV order) — ROI cropped image
        Mat[] channels = roi.Split();
        
        using var rChannel = channels[2];
        using var gChannel = channels[1];
        using var bChannel = channels[0];

        // Calculate statistics for each channel
        var rStats = CalculateChannelStats(rChannel);
        var gStats = CalculateChannelStats(gChannel);
        var bStats = CalculateChannelStats(bChannel);

        analysis.R_Mean = rStats.Mean;
        analysis.G_Mean = gStats.Mean;
        analysis.B_Mean = bStats.Mean;
        analysis.R_StdDev = rStats.StdDev;
        analysis.G_StdDev = gStats.StdDev;
        analysis.B_StdDev = bStats.StdDev;

        // Store histograms
        analysis.R_Histogram = rStats.Histogram;
        analysis.G_Histogram = gStats.Histogram;
        analysis.B_Histogram = bStats.Histogram;

        // Grey-world white balance estimation
        analysis.EstimatedWhiteBalance_R = CalculateGreyWorldGain(rStats.Mean, gStats.Mean, bStats.Mean, 0);
        analysis.EstimatedWhiteBalance_B = CalculateGreyWorldGain(rStats.Mean, gStats.Mean, bStats.Mean, 2);

        // Suggested white balance (corrected)
        analysis.SuggestedWhiteBalance_R = CalculateSuggestedWhiteBalance(rStats.Mean, gStats.Mean);
        analysis.SuggestedWhiteBalance_B = CalculateSuggestedWhiteBalance(bStats.Mean, gStats.Mean);

        // Color cast detection
        var castResult = DetectColorCast(rStats.Mean, gStats.Mean, bStats.Mean);
        analysis.Cast = castResult.Cast;
        analysis.CastIntensity = castResult.Intensity;

        // Fading detection
        analysis.R_Fading = CalculateFading(rStats.Mean);
        analysis.G_Fading = CalculateFading(gStats.Mean);
        analysis.B_Fading = CalculateFading(bStats.Mean);
        analysis.OverallFading = (analysis.R_Fading + analysis.G_Fading + analysis.B_Fading) / 3.0;

        // Overall saturation
        analysis.OverallSaturation = CalculateOverallSaturation(rStats, gStats, bStats);

        // Percentiles and optical density per channel
        analysis.R_P02 = CalculatePercentileFromMat(rChannel, 2.0);
        analysis.R_P25 = CalculatePercentileFromMat(rChannel, 25.0);
        analysis.R_P50 = CalculatePercentileFromMat(rChannel, 50.0);
        analysis.R_P75 = CalculatePercentileFromMat(rChannel, 75.0);
        analysis.R_P98 = CalculatePercentileFromMat(rChannel, 98.0);

        analysis.G_P02 = CalculatePercentileFromMat(gChannel, 2.0);
        analysis.G_P25 = CalculatePercentileFromMat(gChannel, 25.0);
        analysis.G_P50 = CalculatePercentileFromMat(gChannel, 50.0);
        analysis.G_P75 = CalculatePercentileFromMat(gChannel, 75.0);
        analysis.G_P98 = CalculatePercentileFromMat(gChannel, 98.0);

        analysis.B_P02 = CalculatePercentileFromMat(bChannel, 2.0);
        analysis.B_P25 = CalculatePercentileFromMat(bChannel, 25.0);
        analysis.B_P50 = CalculatePercentileFromMat(bChannel, 50.0);
        analysis.B_P75 = CalculatePercentileFromMat(bChannel, 75.0);
        analysis.B_P98 = CalculatePercentileFromMat(bChannel, 98.0);

        // Optical density D = -Log10(val / 65535) for shadows (P02) and highlights (P98)
        analysis.R_DensityShadow = ComputeDensity(analysis.R_P02);
        analysis.R_DensityHighlight = ComputeDensity(analysis.R_P98);
        analysis.G_DensityShadow = ComputeDensity(analysis.G_P02);
        analysis.G_DensityHighlight = ComputeDensity(analysis.G_P98);
        analysis.B_DensityShadow = ComputeDensity(analysis.B_P02);
        analysis.B_DensityHighlight = ComputeDensity(analysis.B_P98);

        // Auto-detect film medium type
        analysis.MediumType = AutoDetectFilmType(analysis);

        // Dispose channel arrays
        foreach (var ch in channels)
            ch.Dispose();

        return analysis;
    }

    private (double Mean, double StdDev, HistogramData Histogram) CalculateChannelStats(Mat channel)
    {
        double mean = Cv2.Mean(channel).Val0;
        
        // Calculate variance manually
        int rows = channel.Rows;
        int cols = channel.Cols;
        double sumSquaredDiff = 0;
        int sampleCount = 0;
        int sampleStep = Math.Max(1, (rows * cols) / 100000);
        
        for (int y = 0; y < rows; y += sampleStep)
        {
            for (int x = 0; x < cols; x += sampleStep)
            {
                double diff = channel.Get<ushort>(y, x) - mean;
                sumSquaredDiff += diff * diff;
                sampleCount++;
            }
        }
        
        double variance = sampleCount > 0 ? sumSquaredDiff / sampleCount : 0;
        double stddev = Math.Sqrt(variance);

        var histogram = ComputeHistogram(channel);

        return (mean, stddev, histogram);
    }

    private HistogramData ComputeHistogram(Mat channel)
    {
        const int bins = 256;
        int[] hist = new int[bins];
        
        // Sample the histogram to avoid processing entire large images
        int rows = channel.Rows;
        int cols = channel.Cols;
        int sampleStep = Math.Max(1, (rows * cols) / 1000000); // Sample up to 1M pixels
        
        int totalSamples = 0;
        long sum = 0;
        long sumSquared = 0;
        int[] localHist = new int[bins];

        for (int y = 0; y < rows; y += sampleStep)
        {
            for (int x = 0; x < cols; x += sampleStep)
            {
                ushort value = channel.Get<ushort>(y, x);
                int bin = Math.Min(bins - 1, (int)(value * bins / BitDepthMax));
                localHist[bin]++;
                sum += value;
                sumSquared += (long)value * value;
                totalSamples++;
            }
        }

        if (totalSamples == 0)
            return new HistogramData(hist, bins, 0, BitDepthMax, 0, 0);

        double mean = sum / (double)totalSamples;
        double variance = (sumSquared / (double)totalSamples) - (mean * mean);
        double stddev = Math.Sqrt(Math.Max(0, variance));

        // Normalize histogram to fit in int array
        double scale = totalSamples > 0 ? bins / (double)totalSamples : 1;
        for (int i = 0; i < bins; i++)
        {
            hist[i] = (int)(localHist[i] * scale);
        }

        return new HistogramData(hist, bins, 0, BitDepthMax, mean, stddev);
    }

    private double CalculateGreyWorldGain(double rMean, double gMean, double bMean, int channelIndex)
    {
        double avgAll = (rMean + gMean + bMean) / 3.0;
        
        if (channelIndex == 0) // R
            return rMean > 0 ? avgAll / rMean : 1.0;
        if (channelIndex == 2) // B
            return bMean > 0 ? avgAll / bMean : 1.0;
        
        return 1.0;
    }

    private double CalculateSuggestedWhiteBalance(double channelMean, double greenMean)
    {
        if (greenMean <= 0) return 1.0;
        return greenMean / channelMean;
    }

    private (ColorCastType Cast, double Intensity) DetectColorCast(double rMean, double gMean, double bMean)
    {
        double avgAll = (rMean + gMean + bMean) / 3.0;
        
        if (avgAll == 0)
            return (ColorCastType.None, 0.0);

        double rDiff = Math.Abs(rMean - avgAll) / avgAll;
        double gDiff = Math.Abs(gMean - avgAll) / avgAll;
        double bDiff = Math.Abs(bMean - avgAll) / avgAll;

        // Find the channel with the largest deviation
        double maxDiff = Math.Max(rDiff, Math.Max(gDiff, bDiff));
        
        if (maxDiff < 0.05) // Less than 5% deviation = no cast
            return (ColorCastType.None, 0.0);

        // Determine cast type based on which channel is most suppressed
        if (rDiff >= gDiff && rDiff >= bDiff)
        {
            // R is suppressed -> Cyan cast (complement of red)
            return (ColorCastType.Cyan, Math.Min(1.0, maxDiff * 2));
        }
        else if (bDiff >= rDiff && bDiff >= gDiff)
        {
            // B is suppressed -> Yellow cast (complement of blue)
            return (ColorCastType.Yellow, Math.Min(1.0, maxDiff * 2));
        }
        else
        {
            // G is suppressed -> Magenta cast (complement of green)
            return (ColorCastType.Magenta, Math.Min(1.0, maxDiff * 2));
        }
    }

    private double CalculateFading(double channelMean)
    {
        // Ideal midpoint for a well-exposed, non-faded image
        double deviation = Math.Abs(channelMean - IdealMidpoint) / IdealMidpoint;
        
        // Closer to ideal = less fading, further = more fading
        double fading = Math.Min(1.0, deviation * 2);
        
        return fading;
    }

    private double CalculateOverallSaturation((double Mean, double StdDev, HistogramData Histogram) r, 
                                              (double Mean, double StdDev, HistogramData Histogram) g, 
                                              (double Mean, double StdDev, HistogramData Histogram) b)
    {
        // Calculate saturation based on channel spread and difference
        double maxStd = Math.Max(r.StdDev, Math.Max(g.StdDev, b.StdDev));
        double maxMean = Math.Max(r.Mean, Math.Max(g.Mean, b.Mean));
        
        if (maxMean == 0) return 0.0;
        
        // Higher std dev relative to mean = higher saturation
        double saturation = maxStd / maxMean;
        
        // Normalize to 0-1 range
        return Math.Min(1.0, saturation);
    }

    /// <summary>
    /// Calculates a percentile value from a 16-bit Mat by sampling pixels.
    /// </summary>
    private static double CalculatePercentileFromMat(Mat channel, double percentile)
    {
        const int bins = 256;
        long[] hist = new long[bins];
        int rows = channel.Rows;
        int cols = channel.Cols;
        int step = Math.Max(1, (rows * cols) / 500000);
        long total = 0;

        for (int y = 0; y < rows; y += step)
        {
            for (int x = 0; x < cols; x += step)
            {
                ushort v = channel.Get<ushort>(y, x);
                int bin = Math.Min(bins - 1, (int)(v * bins / 65535));
                hist[bin]++;
                total++;
            }
        }

        if (total == 0) return 0;

        double target = percentile / 100.0 * total;
        long cumulative = 0;
        for (int i = 0; i < bins; i++)
        {
            cumulative += hist[i];
            if (cumulative >= target)
                return i * (65535.0 / bins);
        }

        return 65535.0;
    }

    /// <summary>
    /// Computes optical density D = -Log10(val / 65535).
    /// </summary>
    private static double ComputeDensity(double value)
    {
        if (value <= 0) return 0.0;
        return -Math.Log10(value / 65535.0);
    }

    /// <summary>
    /// Auto-detects film medium type based on histogram spread.
    /// ColorNegative90s: range (P98-P02) &lt; 42000 OR IQR (P75-P25) &lt; 14000.
    /// Slide80s: wider histogram spread.
    /// </summary>
    private static FilmMediumType AutoDetectFilmType(ColorDegradationAnalysis analysis)
    {
        // Use the channel with the widest spread for classification
        double maxRange = 0;
        double maxIqr = 0;

        foreach ((double p02, double p98, double p25, double p75) in
            new[]
            {
                (analysis.R_P02, analysis.R_P98, analysis.R_P25, analysis.R_P75),
                (analysis.G_P02, analysis.G_P98, analysis.G_P25, analysis.G_P75),
                (analysis.B_P02, analysis.B_P98, analysis.B_P25, analysis.B_P75)
            })
        {
            double range = p98 - p02;
            double iqr = p75 - p25;
            if (range > maxRange) maxRange = range;
            if (iqr > maxIqr) maxIqr = iqr;
        }

        if (maxRange < 42000 || maxIqr < 14000)
            return FilmMediumType.ColorNegative90s;

        return FilmMediumType.Slide80s;
    }
}
