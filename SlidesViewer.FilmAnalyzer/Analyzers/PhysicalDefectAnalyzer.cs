using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Utilities;

namespace SlidesViewer.FilmAnalyzer.Analyzers;

public interface IPhysicalDefectAnalyzer
{
    PhysicalDefectAnalysis Analyze(string filePath);
}

public class PhysicalDefectAnalyzer : IPhysicalDefectAnalyzer
{
    public PhysicalDefectAnalysis Analyze(string filePath)
    {
        using var rawImage = Cv2.ImRead(filePath, ImreadModes.Color | ImreadModes.AnyDepth);


        if (rawImage.Empty())
            throw new InvalidOperationException($"Cannot read image: {filePath}");

        // Normalize to 8-bit 3-channel for OpenCV operations that require CV_8U (Canny, Threshold, etc.)
        // This handles CV_16UC4, CV_16UC1, CV_8UC*, and other formats
        using var image8u = ImageNormalizationUtility.NormalizeTo8BitColor(rawImage);

        var analysis = new PhysicalDefectAnalysis();

        // Convert to grayscale for defect analysis
        using var gray = new Mat();
        Cv2.CvtColor(image8u, gray, ColorConversionCodes.BGR2GRAY);

        // Scratch detection
        analysis.ScratchCount = DetectScratches(gray);

        // Dust detection
        analysis.DustCount = DetectDust(gray);

        // Haze detection
        analysis.HazeLevel = DetectHaze(gray);

        // Base fog density (needs 16-bit image)
        analysis.BaseFogDensity = CalculateBaseFog(rawImage);

        // Local contrast
        analysis.LocalContrast = CalculateLocalContrast(gray);

        // Border detection
        analysis.HasBorder = DetectBorders(image8u);

        return analysis;
    }

    private int DetectScratches(Mat gray)
    {
        // Apply Gaussian blur to reduce noise
        using var blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

        // Canny edge detection
        using var edges = new Mat();
        Cv2.Canny(blurred, edges, 50, 150);

        // Morphological operation with vertical kernel to detect vertical scratches
        using var verticalKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 15));
        using var scratches = new Mat();
        Cv2.MorphologyEx(edges, scratches, MorphTypes.Open, verticalKernel);

        // Find contours
        Point[][]? contours = null;
        scratches.FindContours(out contours!, out HierarchyIndex[]? hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);


        if (contours == null || contours.Length == 0)
            return 0;

        // Count significant contours
        int scratchCount = 0;
        foreach (var contour in contours)
        {
            if (contour.Length == 0) continue;
            double area = Cv2.ContourArea(contour);
            if (area > 50) // Minimum area threshold
                scratchCount++;
        }

        return scratchCount;
    }

    private static int DetectDust(Mat gray)
    {
        // Apply Gaussian blur
        using var blurred = new Mat();
        Cv2.GaussianBlur(gray, blurred, new Size(5, 5), 0);

        // Otsu thresholding
        using var thresh = new Mat();
        Cv2.Threshold(blurred, thresh, 0, 255, ThresholdTypes.Otsu | ThresholdTypes.BinaryInv);

        // Find connected components
        using var labelsMat = new Mat();
        using var statsMat = new Mat();
        using var centroidsMat = new Mat();
        int numLabels = Cv2.ConnectedComponentsWithStats(thresh, labelsMat, statsMat, centroidsMat);

        // Filter by area (small blobs = dust)
        int dustCount = 0;
        for (int i = 1; i < numLabels; i++) // Skip background
        {
            int area = statsMat.At<int>(i, (int)ConnectedComponentsTypes.Area);
            if (area >= 2 && area <= 100) // Dust is small but not noise
                dustCount++;
        }

        return dustCount;
    }

    private double DetectHaze(Mat gray)
    {
        // Local contrast metric using box filter
        const int kernelSize = 31;


        using var localMin = new Mat();
        using var localMax = new Mat();

        // Box filter for smoothing

        Cv2.BoxFilter(gray, localMin, gray.Type(), new Size(kernelSize, kernelSize));
        
        // Dilate for max filter approximation
        Cv2.Dilate(gray, localMax, new Mat());
        Cv2.Erode(localMax, localMax, new Mat());

        // Calculate local contrast using subtract
        using var diff = new Mat();
        Cv2.Absdiff(localMax, localMin, diff);

        double meanContrast = Cv2.Mean(diff).Val0;
        
        // Normalize to 0-1 (lower contrast = more haze)
        // Typical range for clear images: 50-200, hazy: 0-30
        double haze = 1.0 - Math.Min(1.0, meanContrast / 50.0);
        
        return Math.Max(0.0, Math.Min(1.0, haze));
    }

    private double CalculateBaseFog(Mat image)
    {
        // Use normalized 16-bit image; crop 8% from edges to exclude frame artifacts
        using var normalized = ImageNormalizationUtility.NormalizeTo16BitColor(image);
        int crop = (int)(Math.Min(normalized.Rows, normalized.Cols) * 0.08);
        using var cropped = normalized[new Rect(crop, crop, normalized.Cols - 2 * crop, normalized.Rows - 2 * crop)].Clone();

        Mat[] channels = cropped.Split();

        double minFog = double.MaxValue;

        foreach (var ch in channels)
        {
            double p1 = CalculatePercentileFromHistogram(ch, 1.0);
            if (p1 < minFog)
                minFog = p1;
        }

        foreach (var ch in channels)
            ch.Dispose();

        // Normalize to 0-1 range (relative to 16-bit depth)
        return Math.Min(1.0, minFog / 65535.0);
    }

    private static double CalculatePercentileFromHistogram(Mat channel, double percentile)
    {
        // Build a 256-bin histogram from the channel
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

    private double CalculateLocalContrast(Mat gray)
    {
        // Calculate local contrast using variance of local regions
        const int blockSize = 31;
        
        using var localMean = new Mat();
        Cv2.Blur(gray, localMean, new Size(blockSize, blockSize));
        
        // Convert localMean back to original type for subtraction
        using var localMeanConverted = new Mat();
        Cv2.ConvertScaleAbs(localMean, localMeanConverted, 1.0, 0);
        
        // Calculate abs diff
        using var diff = new Mat();
        Cv2.Absdiff(gray, localMeanConverted, diff);
        
        double meanLocalContrast = Cv2.Mean(diff).Val0;
        
        // Normalize to 0-1
        return Math.Min(1.0, meanLocalContrast / 128.0);
    }

    private bool DetectBorders(Mat image)
    {
        // Look for uniform dark borders around the image edges
        int rows = image.Rows;
        int cols = image.Cols;

        // Sample the border regions (5% from each edge)
        int borderSize = Math.Min(rows, cols) / 20;

        // Create a mask for the border: start with all-255, then zero the center
        using var mask = Mat.Ones(rows, cols, MatType.CV_8UC1);
        Rect centerRect = new(borderSize, borderSize, cols - 2 * borderSize, rows - 2 * borderSize);
        Cv2.Rectangle(mask, centerRect, new Scalar(0), -1); // Zero center → mask only borders

        // Calculate mean of border region using the mask directly
        Scalar borderMean = Cv2.Mean(image, mask);

        // Compare average brightness
        using var centerRegion = image[new Rect(cols / 4, rows / 4, cols / 2, rows / 2)];
        Scalar centerMean = Cv2.Mean(centerRegion);

        double borderBrightness = (borderMean.Val0 + borderMean.Val1 + borderMean.Val2) / 3.0;
        double centerBrightness = (centerMean.Val0 + centerMean.Val1 + centerMean.Val2) / 3.0;

        // If border is much darker than center, likely has borders
        return borderBrightness < centerBrightness * 0.3;
    }
}