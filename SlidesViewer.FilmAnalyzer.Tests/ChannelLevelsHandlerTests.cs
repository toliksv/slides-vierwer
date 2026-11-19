using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class ChannelLevelsHandlerTests
{
    [Fact]
    public void Order_ReturnsOne()
    {
        // Arrange & Act
        var handler = new ChannelLevelsHandler();

        // Assert
        Assert.Equal(1, handler.Order);
    }

    [Fact]
    public void Process_ModifiesLevels_WhenNoClippingAndFirstIteration()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.9, p99G: 0.9, p99B: 0.9);
        context.Iteration = 1;

        // Act
        handler.Handle(context);

        // Assert - first iteration always processes
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("Levels"));
        Assert.True(context.ShadowAdjusted);
        Assert.Contains(context.ModifiedKeys, k => k == "Levels");
    }

    [Fact]
    public void Process_ModifiesLevels_WhenClippingDetected()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0, clipHiB: 0, // Red has clipping
            p01R: 0.05, p01G: 0.05, p01B: 0.05,
            p99R: 0.95, p99G: 0.95, p99B: 0.95);

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("Levels"));
        Assert.True(context.ShadowAdjusted);
        Assert.Contains(context.ModifiedKeys, k => k == "Levels");

        // Verify shadow values are clamped to [0.01, 0.25]
        Assert.InRange(context.Adjustments.Exposure.ShadowR, 0.01, 0.25);
        Assert.InRange(context.Adjustments.Exposure.ShadowG, 0.01, 0.25);
        Assert.InRange(context.Adjustments.Exposure.ShadowB, 0.01, 0.25);

        // Verify highlight values are clamped to [0.88, 1.00]
        Assert.InRange(context.Adjustments.Exposure.HighlightR, 0.88, 1.00);
        Assert.InRange(context.Adjustments.Exposure.HighlightG, 0.88, 1.00);
        Assert.InRange(context.Adjustments.Exposure.HighlightB, 0.88, 1.00);
    }

    [Fact]
    public void Process_ModifiesLevels_WhenClippingOnBlueChannel()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0.02,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.9, p99G: 0.9, p99B: 0.98);

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.True(context.ShadowAdjusted);
    }

    [Fact]
    public void Process_ModifiesLevels_WhenClippingOnGreenChannel()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0.015, clipHiB: 0,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.9, p99G: 0.95, p99B: 0.9);

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.True(context.ShadowAdjusted);
    }

    [Fact]
    public void Process_DoesNotModify_WhenNoClippingOnAnyChannel()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.9, p99G: 0.9, p99B: 0.9);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
        Assert.False(context.ShadowAdjusted);
    }

    [Fact]
    public void Process_ShadowValuesAreClampedToMax025()
    {
        // Arrange - P01 values that would produce shadow > 0.25
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0.01, clipHiB: 0.01,
            p01R: 0.3, p01G: 0.3, p01B: 0.3, // 0.3 * 0.9 = 0.27, clamped to 0.25
            p99R: 0.9, p99G: 0.9, p99B: 0.9);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(0.25, context.Adjustments.Exposure.ShadowR);
        Assert.Equal(0.25, context.Adjustments.Exposure.ShadowG);
        Assert.Equal(0.25, context.Adjustments.Exposure.ShadowB);
    }

    [Fact]
    public void Process_ShadowValuesAreClampedToMin001()
    {
        // Arrange - P01 values that would produce shadow < 0.01
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0.01, clipHiB: 0.01,
            p01R: 0.005, p01G: 0.005, p01B: 0.005, // 0.005 * 0.9 = 0.0045, clamped to 0.01
            p99R: 0.9, p99G: 0.9, p99B: 0.9);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(0.01, context.Adjustments.Exposure.ShadowR);
        Assert.Equal(0.01, context.Adjustments.Exposure.ShadowG);
        Assert.Equal(0.01, context.Adjustments.Exposure.ShadowB);
    }

    [Fact]
    public void Process_HighlightValuesAreClampedToMax100()
    {
        // Arrange - P99 values that would produce highlight > 1.00
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0.01, clipHiB: 0.01,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 1.0, p99G: 1.0, p99B: 1.0); // 1.0 * 1.02 = 1.02, clamped to 1.00

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(1.00, context.Adjustments.Exposure.HighlightR);
        Assert.Equal(1.00, context.Adjustments.Exposure.HighlightG);
        Assert.Equal(1.00, context.Adjustments.Exposure.HighlightB);
    }

    [Fact]
    public void Process_HighlightValuesAreClampedToMin088()
    {
        // Arrange - P99 values that would produce highlight < 0.88
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0.01, clipHiB: 0.01,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.8, p99G: 0.8, p99B: 0.8); // 0.8 * 1.02 = 0.816, clamped to 0.88

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(0.88, context.Adjustments.Exposure.HighlightR);
        Assert.Equal(0.88, context.Adjustments.Exposure.HighlightG);
        Assert.Equal(0.88, context.Adjustments.Exposure.HighlightB);
    }

    [Fact]
    public void Process_LogContainsShadowAndHighlightValues()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0, clipHiB: 0,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.95, p99G: 0.95, p99B: 0.95);

        // Act
        handler.Handle(context);

        // Assert
        var logEntry = Assert.Single(context.Log.Where(l => l.Contains("Levels")));
        Assert.Contains("S=[", logEntry);
        Assert.Contains("H=[", logEntry);
    }

    [Fact]
    public void Process_IterationGreaterThan1_DoesNotModify_WhenNoClipping()
    {
        // Arrange
        var handler = new ChannelLevelsHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            p01R: 0.1, p01G: 0.1, p01B: 0.1,
            p99R: 0.9, p99G: 0.9, p99B: 0.9);
        context.Iteration = 3;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    private static OptimizationContext CreateContext(
        double clipHiR, double clipHiG, double clipHiB,
        double p01R, double p01G, double p01B,
        double p99R, double p99G, double p99B)
    {
        return new OptimizationContext
        {
            Adjustments = new CaptureOneAdjustments(),
            Metrics = new FrameMetrics
            {
                ClipHiR = clipHiR,
                ClipHiG = clipHiG,
                ClipHiB = clipHiB,
                P01_R = p01R,
                P01_G = p01G,
                P01_B = p01B,
                P99_R = p99R,
                P99_G = p99G,
                P99_B = p99B
            }
        };
    }
}
