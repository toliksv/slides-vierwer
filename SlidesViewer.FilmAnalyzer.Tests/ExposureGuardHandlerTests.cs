using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class ExposureGuardHandlerTests
{
    [Fact]
    public void Order_ReturnsTwo()
    {
        // Arrange & Act
        var handler = new ExposureGuardHandler();

        // Assert
        Assert.Equal(2, handler.Order);
    }

    [Fact]
    public void Process_DoesNotModify_WhenNoClipping()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
        Assert.Equal(0, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Process_ReducesExposure_WhenHighlightClippingDetected()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0, clipHiB: 0,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("Exposure"));
        Assert.Contains(context.Log, l => l.Contains("-0.10"));
        Assert.Equal(-0.10, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Process_ReducesExposure_WhenAllChannelsHaveHighlightClipping()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0.02, clipHiB: 0.015,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(-0.10, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Process_IncreasesExposure_WhenShadowClippingDetectedAndShadowNotAdjusted()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            clipLoR: 0.1, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = false; // Shadow not yet adjusted

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("Exposure"));
        Assert.Contains(context.Log, l => l.Contains("+0.10"));
        Assert.Equal(0.10, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Process_DoesNotIncreaseExposure_WhenShadowClippingButShadowAlreadyAdjusted()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            clipLoR: 0.1, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true; // Shadow already adjusted

        // Act
        handler.Handle(context);

        // Assert - no log entries because neither condition is met
        Assert.Empty(context.Log);
        Assert.Equal(0, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Process_DoesNotIncreaseExposure_WhenShadowClippingBelowThreshold()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            clipLoR: 0.03, clipLoG: 0, clipLoB: 0); // Below 0.05 threshold
        context.ShadowAdjusted = false;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_DoesNotIncreaseExposure_WhenHighlightClippingBelowThreshold()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0.003, clipHiG: 0, clipHiB: 0, // Below 0.005 threshold
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_PrioritizesHighlightReductionOverShadowIncrease()
    {
        // Arrange - both clipping conditions met
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0, clipHiB: 0,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert - only highlight reduction should happen
        Assert.Single(context.Log.Where(l => l.Contains("Exposure")));
        Assert.Contains(context.Log, l => l.Contains("-0.10"));
    }

    [Fact]
    public void Process_LogContainsClippingValue()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0.012,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        var logEntry = context.Log.First(l => l.Contains("Exposure"));
        Assert.Contains("clipHi=", logEntry);
    }

    [Fact]
    public void Process_LogContainsShadowClippingValue()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0, clipHiG: 0, clipHiB: 0,
            clipLoR: 0, clipLoG: 0, clipLoB: 0.08);
        context.ShadowAdjusted = false;

        // Act
        handler.Handle(context);

        // Assert
        var logEntry = context.Log.First(l => l.Contains("Exposure"));
        Assert.Contains("clipLo=", logEntry);
    }

    [Fact]
    public void Process_WithNegativeInitialExposure_ReducesFurther()
    {
        // Arrange
        var handler = new ExposureGuardHandler();
        var context = CreateContext(
            clipHiR: 0.01, clipHiG: 0, clipHiB: 0,
            clipLoR: 0, clipLoG: 0, clipLoB: 0);
        context.Adjustments.Exposure.Exposure = -0.5;
        context.ShadowAdjusted = true;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(-0.60, context.Adjustments.Exposure.Exposure);
    }

    private static OptimizationContext CreateContext(
        double clipHiR, double clipHiG, double clipHiB,
        double clipLoR, double clipLoG, double clipLoB)
    {
        return new OptimizationContext
        {
            Adjustments = new CaptureOneAdjustments(),
            Metrics = new FrameMetrics
            {
                ClipHiR = clipHiR,
                ClipHiG = clipHiG,
                ClipHiB = clipHiB,
                ClipLoR = clipLoR,
                ClipLoG = clipLoG,
                ClipLoB = clipLoB
            }
        };
    }
}
