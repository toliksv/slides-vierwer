using OpenCvSharp;

namespace SlidesViewer.FilmAnalyzer.Utilities;

/// <summary>
/// Utility methods for normalizing image formats, especially for high-bit-depth TIFF files.
/// </summary>
public static class ImageNormalizationUtility
{
    /// <summary>
    /// Normalizes an image to 16-bit 3-channel (CV_16UC3) format.
    /// Handles CV_16UC1, CV_16UC3, CV_16UC4, CV_8UC1, CV_8UC3, CV_8UC4, and other common formats.
    /// </summary>
    /// <param name="image">Source image (must not be empty).</param>
    /// <returns>Image in CV_16UC3 format.</returns>
    public static Mat NormalizeTo16BitColor(Mat image)
    {
        if (image.Empty())
            throw new InvalidOperationException("Cannot normalize empty image.");

        if (image.Type() == MatType.CV_16UC3)
        {
            // Already in target format
            return image.Clone();
        }

        if (image.Type() == MatType.CV_16UC4)
        {
            // Remove alpha channel
            var target = new Mat();
            Cv2.CvtColor(image, target, ColorConversionCodes.BGRA2BGR);
            return target;
        }

        if (image.Type() == MatType.CV_16UC1)
        {
            // Convert single-channel to 3-channel
            var target = new Mat();
            Cv2.CvtColor(image, target, ColorConversionCodes.GRAY2BGR);
            return target;
        }

        if (image.Type() == MatType.CV_8UC3)
        {
            // Upscale 8-bit to 16-bit
            var target = new Mat();
            image.ConvertTo(target, MatType.CV_16UC3, 256.0);
            return target;
        }

        if (image.Type() == MatType.CV_8UC4)
        {
            // Remove alpha, then upscale to 16-bit
            using var bgr8 = new Mat();
            Cv2.CvtColor(image, bgr8, ColorConversionCodes.BGRA2BGR);
            var target = new Mat();
            bgr8.ConvertTo(target, MatType.CV_16UC3, 256.0);
            return target;
        }

        if (image.Type() == MatType.CV_8UC1)
        {
            // Convert gray to BGR, then upscale to 16-bit
            using var bgr8Gray = new Mat();
            Cv2.CvtColor(image, bgr8Gray, ColorConversionCodes.GRAY2BGR);
            var target = new Mat();
            bgr8Gray.ConvertTo(target, MatType.CV_16UC3, 256.0);
            return target;
        }

        if (image.Type() == MatType.CV_32FC3)
        {
            // Normalize float32 to 16-bit
            var target = new Mat();
            image.ConvertTo(target, MatType.CV_16UC3, 65535.0);
            return target;
        }

        if (image.Type() == MatType.CV_32FC1)
        {
            using var gray32 = new Mat();
            image.ConvertTo(gray32, MatType.CV_16UC1, 65535.0);
            var target = new Mat();
            Cv2.CvtColor(gray32, target, ColorConversionCodes.GRAY2BGR);
            return target;
        }

        // Fallback: try to convert to BGR first, then to 16-bit
        Mat workingImage;
        if (image.Channels() == 1)
        {
            using var temp = new Mat();
            Cv2.CvtColor(image, temp, ColorConversionCodes.GRAY2BGR);
            workingImage = temp;
        }
        else if (image.Channels() == 4)
        {
            using var temp = new Mat();
            Cv2.CvtColor(image, temp, ColorConversionCodes.BGRA2BGR);
            workingImage = temp;
        }
        else
        {
            workingImage = image.Clone();
        }

        var targetFallback = new Mat();
        double scale = workingImage.Depth() == 0 ? 256.0 : 1.0; // Depth 0 = Cv8U
        workingImage.ConvertTo(targetFallback, MatType.CV_16UC3, scale);
        return targetFallback;
    }

    /// <summary>
    /// Normalizes an image to 8-bit 3-channel (CV_8UC3) format for OpenCV operations
    /// that require CV_8U (e.g., Canny, Threshold, ConnectedComponents).
    /// </summary>
    /// <param name="image">Source image (any format).</param>
    /// <returns>Image in CV_8UC3 format.</returns>
    public static Mat NormalizeTo8BitColor(Mat image)
    {
        if (image.Empty())
            throw new InvalidOperationException("Cannot normalize empty image.");

        // First normalize to 16-bit color, then convert to 8-bit
        using var normalized16 = NormalizeTo16BitColor(image);

        if (normalized16.Type() == MatType.CV_16UC3)
        {
            // Scale down from 16-bit to 8-bit
            var target = new Mat();
            normalized16.ConvertTo(target, MatType.CV_8UC3, 1.0 / 256.0);
            return target;
        }

        return normalized16.Clone();
    }
}
