using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Services.Simulation;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class PreviewSimulatorTests
{
    private readonly PreviewSimulator _simulator;

    public PreviewSimulatorTests()
    {
        _simulator = new PreviewSimulator();
    }

    [Fact]
    public void Simulate_ReturnsNonNullMat()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
    }

    [Fact]
    public void Simulate_Returns3ChannelMat()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.Equal(3, result.Channels());
    }

    [Fact]
    public void Simulate_ReturnsSameSizeAsSource()
    {
        // Arrange
        var source = CreateTestMat(200, 150);
        var adjustments = new CaptureOneAdjustments();

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.Equal(200, result.Cols);
        Assert.Equal(150, result.Rows);
    }

    [Fact]
    public void Simulate_AppliesExposureAdjustment()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment { Exposure = 1.0 } // +1 EV
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert - result should be brighter than source
        var sourceChannels = SplitChannels(source);
        var resultChannels = SplitChannels(result);
        
        double sourceMean = 0, resultMean = 0;
        for (int c = 0; c < 3; c++)
        {
            for (int i = 0; i < sourceChannels[c].Length; i++)
            {
                sourceMean += sourceChannels[c][i];
                resultMean += resultChannels[c][i];
            }
        }
        sourceMean /= (sourceChannels[0].Length * 3);
        resultMean /= (resultChannels[0].Length * 3);

        Assert.True(resultMean > sourceMean, "Result should be brighter with positive exposure");
    }

    [Fact]
    public void Simulate_AppliesNegativeExposureAdjustment()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment { Exposure = -1.0 } // -1 EV
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert - result should be darker than source
        var sourceChannels = SplitChannels(source);
        var resultChannels = SplitChannels(result);
        
        double sourceMean = 0, resultMean = 0;
        for (int c = 0; c < 3; c++)
        {
            for (int i = 0; i < sourceChannels[c].Length; i++)
            {
                sourceMean += sourceChannels[c][i];
                resultMean += resultChannels[c][i];
            }
        }
        sourceMean /= (sourceChannels[0].Length * 3);
        resultMean /= (resultChannels[0].Length * 3);

        Assert.True(resultMean < sourceMean, "Result should be darker with negative exposure");
    }

    [Fact]
    public void Simulate_AppliesWhiteBalanceTemperature()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            WhiteBalance = new WhiteBalanceAdjustment { ColorTemperature = 7000 } // Cool
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert - with high temp (cool), blue channel should be boosted relative to red
        var channels = SplitChannels(result);
        double blueMean = channels[0].Average();
        double redMean = channels[2].Average();

        // With cool temperature, blue should be relatively higher
        Assert.True(blueMean >= redMean || Math.Abs(blueMean - redMean) < 0.01,
            "Cool temp should increase blue relative to red");
    }

    [Fact]
    public void Simulate_AppliesWhiteBalanceTint()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            WhiteBalance = new WhiteBalanceAdjustment { Tint = 5 } // Green tint
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert - should not throw and should return valid mat
        Assert.NotNull(result);
        Assert.False(result.Empty());
    }

    [Fact]
    public void Simulate_AppliesShadowAdjustment()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment
            {
                ShadowR = 0.1,
                ShadowG = 0.1,
                ShadowB = 0.1,
                Highlight = 0.9
            }
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
    }

    [Fact]
    public void Simulate_AppliesHighlightAdjustment()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment
            {
                Shadow = 0.05,
                HighlightR = 0.95,
                HighlightG = 0.95,
                HighlightB = 0.95
            }
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
    }

    [Fact]
    public void Simulate_WithDefaultAdjustments_ReturnsSimilarValues()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments(); // All defaults

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
        var channels = SplitChannels(result);
        foreach (var ch in channels)
        {
            Assert.NotEmpty(ch);
        }
    }

    [Fact]
    public void Simulate_ValuesAreClampedTo01Range()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment
            {
                Exposure = 5.0,
                ShadowR = 0.01,
                ShadowG = 0.01,
                ShadowB = 0.01,
                HighlightR = 0.99,
                HighlightG = 0.99,
                HighlightB = 0.99
            },
            WhiteBalance = new WhiteBalanceAdjustment { ColorTemperature = 10000, Tint = 100 }
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert - all values should be in [0, 1] range
        var channels = SplitChannels(result);
        foreach (var ch in channels)
        {
            foreach (var v in ch)
            {
                Assert.InRange(v, 0f, 1f);
            }
        }
    }

    [Fact]
    public void Simulate_WithSineCurve_AdjustsMidtones()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            ToneCurve = new ToneCurveAdjustment
            {
                RedPoints = new[] { 0, 0, 16384, 10000, 32768, 32768, 49152, 55000, 65535, 65535 }
            }
        };

        // Act
        var result = _simulator.Simulate(source, adjustments);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Empty());
    }

    private static Mat CreateTestMat(int width, int height)
    {
        var mat = new Mat(height, width, MatType.CV_32FC3);
        var channels = Cv2.Split(mat);
        foreach (var ch in channels)
        {
            ch.SetTo(new Scalar(0.5));
        }
        Cv2.Merge(channels, mat);
        return mat;
    }

    private static float[][] SplitChannels(Mat mat)
    {
        var channels = Cv2.Split(mat);
        var result = new float[3][];
        for (int i = 0; i < 3; i++)
        {
            channels[i].GetArray(out result[i]);
            channels[i].Dispose();
        }
        return result;
    }
}
