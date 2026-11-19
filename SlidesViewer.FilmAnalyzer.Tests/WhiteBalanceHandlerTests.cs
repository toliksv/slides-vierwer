using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class WhiteBalanceHandlerTests
{
    [Fact]
    public void Order_ReturnsThree()
    {
        // Arrange & Act
        var handler = new WhiteBalanceHandler();

        // Assert
        Assert.Equal(3, handler.Order);
    }

    [Fact]
    public void Process_DoesNotModify_WhenWhiteBalanceIsGood()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.5, p50B: 0.5);
        context.WhiteBalanceAdjusted = false;

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
        Assert.Equal(0, context.Adjustments.WhiteBalance.ColorTemperature);
        Assert.Equal(0, context.Adjustments.WhiteBalance.Tint);
    }

    [Fact]
    public void Process_IncreasesTemperature_WhenBlueExceedsRed()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.45, p50G: 0.5, p50B: 0.55); // blueRedDiff = 0.10 > 0.03

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("WhiteBalance"));
        Assert.True(context.Adjustments.WhiteBalance.ColorTemperature > 0);
        Assert.True(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void Process_DecreasesTemperature_WhenRedExceedsBlue()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.55, p50G: 0.5, p50B: 0.45); // blueRedDiff = -0.10 < -0.03

        // Act
        handler.Handle(context);

        // Assert - handler clamps to [5000, 6000], default is 0 so result is 5000
        Assert.NotEmpty(context.Log);
        Assert.Equal(5000, context.Adjustments.WhiteBalance.ColorTemperature);
        Assert.True(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void Process_AdjustsTemperatureBy150Steps()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.4, p50G: 0.5, p50B: 0.6); // large difference
        // Start with a valid initial value so +150 doesn't get clamped
        context.Adjustments.WhiteBalance.ColorTemperature = 5850;

        // Act
        handler.Handle(context);

        // Assert - 5850 + 150 = 6000 (clamped to max)
        Assert.Equal(6000, context.Adjustments.WhiteBalance.ColorTemperature);
    }

    [Fact]
    public void Process_AdjustsTint_WhenGreenCastDetected()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.58, p50B: 0.5); // greenCast = 0.08 > 0.03

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.True(context.Adjustments.WhiteBalance.Tint > 0);
        Assert.True(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void Process_AdjustsTintNegative_WhenGreenDeficitDetected()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.42, p50B: 0.5); // greenCast = -0.08 < -0.03

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.True(context.Adjustments.WhiteBalance.Tint < 0);
    }

    [Fact]
    public void Process_AdjustsTintBy1Step()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.55, p50B: 0.5);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Equal(1.0, context.Adjustments.WhiteBalance.Tint);
    }

    [Fact]
    public void Process_AdjustsBothTemperatureAndTint_WhenBothConditionsMet()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.42, p50G: 0.58, p50B: 0.55); // Both conditions > 0.03

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.True(context.Adjustments.WhiteBalance.ColorTemperature > 0);
        Assert.True(context.Adjustments.WhiteBalance.Tint > 0);
    }

    [Fact]
    public void Process_DoesNotModify_WhenDiffJustBelowThreshold()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.485, p50G: 0.5, p50B: 0.485); // blueRedDiff = 0.0 exactly

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_DoesNotModify_WhenGreenCastJustBelowThreshold()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.52, p50B: 0.5); // greenCast = 0.02 < 0.03

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_LogContainsTemperatureAndTintValues()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.45, p50G: 0.5, p50B: 0.55);

        // Act
        handler.Handle(context);

        // Assert
        var logEntry = context.Log.First(l => l.Contains("WhiteBalance"));
        Assert.Contains("CT=", logEntry);
        Assert.Contains("K", logEntry);
        Assert.Contains("Tint=", logEntry);
    }

    [Fact]
    public void Process_SetsWhiteBalanceAdjustedFlag()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.45, p50G: 0.5, p50B: 0.55);

        // Act
        handler.Handle(context);

        // Assert
        Assert.True(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void Process_DoesNotSetWhiteBalanceAdjusted_WhenNoAdjustmentMade()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.5, p50B: 0.5);

        // Act
        handler.Handle(context);

        // Assert
        Assert.False(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void Process_ColorTemperatureIsClampedToMax6000()
    {
        // Arrange - start at default 0, add 150 = 150 (well within bounds)
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.45, p50G: 0.5, p50B: 0.55);
        context.Adjustments.WhiteBalance.ColorTemperature = 5900;

        // Act
        handler.Handle(context);

        // Assert - 5900 + 150 = 6050, clamped to 6000
        Assert.Equal(6000, context.Adjustments.WhiteBalance.ColorTemperature);
    }

    [Fact]
    public void Process_ColorTemperatureIsClampedToMin5000()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.55, p50G: 0.5, p50B: 0.45);
        context.Adjustments.WhiteBalance.ColorTemperature = 5050;

        // Act
        handler.Handle(context);

        // Assert - 5050 - 150 = 4900, clamped to 5000
        Assert.Equal(5000, context.Adjustments.WhiteBalance.ColorTemperature);
    }

    [Fact]
    public void Process_TintIsClampedToMax5()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.55, p50B: 0.5);
        context.Adjustments.WhiteBalance.Tint = 5;

        // Act
        handler.Handle(context);

        // Assert - 5 + 1 = 6, clamped to 5
        Assert.Equal(5, context.Adjustments.WhiteBalance.Tint);
    }

    [Fact]
    public void Process_TintIsClampedToMinNegative5()
    {
        // Arrange
        var handler = new WhiteBalanceHandler();
        var context = CreateContext(
            p50R: 0.5, p50G: 0.45, p50B: 0.5);
        context.Adjustments.WhiteBalance.Tint = -5;

        // Act
        handler.Handle(context);

        // Assert - -5 - 1 = -6, clamped to -5
        Assert.Equal(-5, context.Adjustments.WhiteBalance.Tint);
    }

    private static OptimizationContext CreateContext(
        double p50R, double p50G, double p50B)
    {
        return new OptimizationContext
        {
            Adjustments = new CaptureOneAdjustments(),
            Metrics = new FrameMetrics
            {
                P50_R = p50R,
                P50_G = p50G,
                P50_B = p50B
            }
        };
    }
}
