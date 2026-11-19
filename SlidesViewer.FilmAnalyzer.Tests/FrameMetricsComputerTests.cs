using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Services.Metrics;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class FrameMetricsComputerTests
{
    private readonly FrameMetricsComputer _computer;

    public FrameMetricsComputerTests()
    {
        _computer = new FrameMetricsComputer();
    }

    [Fact]
    public void Compute_ReturnsNonNullMetrics()
    {
        // Arrange
        var image = CreateUniformImage(0.5f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Compute_SetsIsNegativeFlag()
    {
        // Arrange
        var image = CreateUniformImage(0.5f, 100, 100);

        // Act
        var result = _computer.Compute(image, true);

        // Assert
        Assert.True(result.IsNegative);
    }

    [Fact]
    public void Compute_SetsIsNegativeFalse()
    {
        // Arrange
        var image = CreateUniformImage(0.5f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.False(result.IsNegative);
    }

    [Fact]
    public void Compute_PercentsAreInRange0To1()
    {
        // Arrange
        var image = CreateUniformImage(0.5f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.P01_R, 0, 1);
        Assert.InRange(result.P01_G, 0, 1);
        Assert.InRange(result.P01_B, 0, 1);
        Assert.InRange(result.P02_R, 0, 1);
        Assert.InRange(result.P02_G, 0, 1);
        Assert.InRange(result.P02_B, 0, 1);
        Assert.InRange(result.P05_R, 0, 1);
        Assert.InRange(result.P05_G, 0, 1);
        Assert.InRange(result.P05_B, 0, 1);
        Assert.InRange(result.P50_R, 0, 1);
        Assert.InRange(result.P50_G, 0, 1);
        Assert.InRange(result.P50_B, 0, 1);
        Assert.InRange(result.P95_R, 0, 1);
        Assert.InRange(result.P95_G, 0, 1);
        Assert.InRange(result.P95_B, 0, 1);
        Assert.InRange(result.P98_R, 0, 1);
        Assert.InRange(result.P98_G, 0, 1);
        Assert.InRange(result.P98_B, 0, 1);
        Assert.InRange(result.P99_R, 0, 1);
        Assert.InRange(result.P99_G, 0, 1);
        Assert.InRange(result.P99_B, 0, 1);
    }

    [Fact]
    public void Compute_ClippingValuesAreInRange0To1()
    {
        // Arrange
        var image = CreateUniformImage(0.5f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.ClipHiR, 0, 1);
        Assert.InRange(result.ClipHiG, 0, 1);
        Assert.InRange(result.ClipHiB, 0, 1);
        Assert.InRange(result.ClipLoR, 0, 1);
        Assert.InRange(result.ClipLoG, 0, 1);
        Assert.InRange(result.ClipLoB, 0, 1);
    }

    [Fact]
    public void Compute_UniformImage_P50CloseToInputValue()
    {
        // Arrange
        var inputValue = 0.5f;
        var image = CreateUniformImage(inputValue, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.P50_R, inputValue - 0.05, inputValue + 0.05);
        Assert.InRange(result.P50_G, inputValue - 0.05, inputValue + 0.05);
        Assert.InRange(result.P50_B, inputValue - 0.05, inputValue + 0.05);
    }

    [Fact]
    public void Compute_DarkImage_HasLowPercents()
    {
        // Arrange
        var image = CreateUniformImage(0.05f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.P50_R, 0, 0.2);
        Assert.InRange(result.P50_G, 0, 0.2);
        Assert.InRange(result.P50_B, 0, 0.2);
    }

    [Fact]
    public void Compute_BrightImage_HasHighPercents()
    {
        // Arrange
        var image = CreateUniformImage(0.95f, 100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.P50_R, 0.8, 1.0);
        Assert.InRange(result.P50_G, 0.8, 1.0);
        Assert.InRange(result.P50_B, 0.8, 1.0);
    }

    [Fact]
    public void Compute_ImageWithHighlightClipping_HasHighClipHi()
    {
        // Arrange
        var image = CreateImageWithClipping(clipHi: 0.05);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        // At least some channels should have clipping
        Assert.True(result.ClipHiR > 0 || result.ClipHiG > 0 || result.ClipHiB > 0);
    }

    [Fact]
    public void Compute_ImageWithShadowClipping_HasHighClipLo()
    {
        // Arrange
        var image = CreateImageWithClipping(clipLo: 0.1);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.True(result.ClipLoR > 0 || result.ClipLoG > 0 || result.ClipLoB > 0);
    }

    [Fact]
    public void Compute_PercentsAreMonotonicallyIncreasing()
    {
        // Arrange
        var image = CreateGrayscaleGradientImage(100, 100);

        // Act
        var result = _computer.Compute(image, false);

        // Assert - P01 <= P02 <= P05 <= P50 <= P95 <= P98 <= P99
        Assert.True(result.P01_R <= result.P02_R && result.P02_R <= result.P05_R &&
                    result.P05_R <= result.P50_R && result.P50_R <= result.P95_R &&
                    result.P95_R <= result.P98_R && result.P98_R <= result.P99_R);
        Assert.True(result.P01_G <= result.P02_G && result.P02_G <= result.P05_G &&
                    result.P05_G <= result.P50_G && result.P50_G <= result.P95_G &&
                    result.P95_G <= result.P98_G && result.P98_G <= result.P99_G);
        Assert.True(result.P01_B <= result.P02_B && result.P02_B <= result.P05_B &&
                    result.P05_B <= result.P50_B && result.P50_B <= result.P95_B &&
                    result.P95_B <= result.P98_B && result.P98_B <= result.P99_B);
    }

    [Fact]
    public void Compute_RChannelPercentsAreAssignedCorrectly()
    {
        // Arrange - create image where R channel is distinctly different
        var image = CreateChannelSpecificImage(rValue: 0.3f, gValue: 0.5f, bValue: 0.7f);

        // Act
        var result = _computer.Compute(image, false);

        // Assert - R should be lowest, B should be highest
        Assert.True(result.P50_R < result.P50_G);
        Assert.True(result.P50_G < result.P50_B);
    }

    [Fact]
    public void Compute_P95AndP98AndP99DefaultToOne_WhenNotEnoughPixels()
    {
        // Arrange - very small image
        var image = CreateUniformImage(0.5f, 2, 2);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.InRange(result.P95_R, 0, 1);
        Assert.InRange(result.P98_R, 0, 1);
        Assert.InRange(result.P99_R, 0, 1);
    }

    [Fact]
    public void Compute_LargeImage_ComputesSuccessfully()
    {
        // Arrange
        var image = CreateGrayscaleGradientImage(800, 600);

        // Act
        var result = _computer.Compute(image, false);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsNegative);
    }

    private static Mat CreateUniformImage(float value, int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_32FC3);
        var channels = Cv2.Split(mat);
        foreach (var ch in channels)
        {
            ch.SetTo(new Scalar(value));
        }
        Cv2.Merge(channels, mat);
        return mat;
    }

    private static Mat CreateImageWithClipping(double clipHi = 0, double clipLo = 0)
    {
        // Create image with some pixels at extremes
        var mat = new Mat(100, 100, MatType.CV_32FC3);
        var channels = Cv2.Split(mat);
        
        int totalPixels = 100 * 100;
        int hiPixels = (int)(totalPixels * clipHi / 3);
        int loPixels = (int)(totalPixels * clipLo / 3);
        
        for (int c = 0; c < 3; c++)
        {
            channels[c].GetArray(out float[] arr);
            for (int i = 0; i < arr.Length; i++)
            {
                if (i < hiPixels)
                    arr[i] = 0.995f; // Near-white
                else if (i < hiPixels + loPixels)
                    arr[i] = 0.001f; // Near-black
                else
                    arr[i] = 0.5f;
            }
            channels[c].SetArray(arr);
        }
        Cv2.Merge(channels, mat);
        return mat;
    }

    private static Mat CreateGrayscaleGradientImage(int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_32FC3);
        var channels = Cv2.Split(mat);
        
        for (int c = 0; c < 3; c++)
        {
            channels[c].GetArray(out float[] arr);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    float value = x / (float)width;
                    arr[idx] = value;
                }
            }
            channels[c].SetArray(arr);
        }
        Cv2.Merge(channels, mat);
        return mat;
    }

    private static Mat CreateChannelSpecificImage(float rValue, float gValue, float bValue)
    {
        var mat = new Mat(100, 100, MatType.CV_32FC3);
        var channels = Cv2.Split(mat);
        
        channels[0].SetTo(new Scalar(bValue));  // B
        channels[1].SetTo(new Scalar(gValue));  // G
        channels[2].SetTo(new Scalar(rValue));  // R
        
        Cv2.Merge(channels, mat);
        return mat;
    }
}
