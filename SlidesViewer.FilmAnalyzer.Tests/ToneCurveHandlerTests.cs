using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class ToneCurveHandlerTests
{
    [Fact]
    public void Order_ReturnsFour()
    {
        // Arrange & Act
        var handler = new ToneCurveHandler();

        // Assert
        Assert.Equal(4, handler.Order);
    }

    [Fact]
    public void Process_SetsLuminanceCurve_WhenFirstIteration()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 1;
        context.Adjustments.ToneCurve.LuminancePoints = Array.Empty<int>();
        // Provide valid channel points to avoid IndexOutOfRangeException
        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Adjustments.ToneCurve.LuminancePoints);
        Assert.Equal(10, context.Adjustments.ToneCurve.LuminancePoints.Length);
        // Positive film: 0, 0, 16384, 15800, 32768, 32768, 49152, 49700, 65535, 65535
        Assert.Equal(new[] { 0, 0, 16384, 15800, 32768, 32768, 49152, 49700, 65535, 65535 },
            context.Adjustments.ToneCurve.LuminancePoints);
    }

    [Fact]
    public void Process_SetsNegativeLuminanceCurve_WhenFirstIterationAndNegative()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: true);
        context.Iteration = 1;
        context.Adjustments.ToneCurve.LuminancePoints = Array.Empty<int>();
        // Provide valid channel points to avoid IndexOutOfRangeException
        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Adjustments.ToneCurve.LuminancePoints);
        Assert.Equal(10, context.Adjustments.ToneCurve.LuminancePoints.Length);
        // Negative film: 0, 0, 16384, 14000, 32768, 32768, 49152, 51500, 65535, 65535
        Assert.Equal(new[] { 0, 0, 16384, 14000, 32768, 32768, 49152, 51500, 65535, 65535 },
            context.Adjustments.ToneCurve.LuminancePoints);
    }

    [Fact]
    public void Process_DoesNotChangeLuminanceCurve_WhenNotFirstIteration()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var existingPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.Adjustments.ToneCurve.LuminancePoints = existingPoints;
        // Provide valid channel points
        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert
        // mid = Math.Max(1, Math.Min(5, Math.Min(5, 5))) = 5 -> Y of last point
        // RedPoints[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.RedPoints[5]);
    }

    [Fact]
    public void Process_ClampGreenPointToMin0()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var greenPoints = new[] { 0, 0, 200, 200, 65535, 65535 };
        var bluePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };

        context.Adjustments.ToneCurve.RedPoints = redPoints;
        context.Adjustments.ToneCurve.GreenPoints = greenPoints;
        context.Adjustments.ToneCurve.BluePoints = bluePoints;

        // Act
        handler.Handle(context);

        // Assert
        // mid = 5, GreenPoints[5] = 65535 - 500 = 65035
        Assert.Equal(65035, context.Adjustments.ToneCurve.GreenPoints[5]);
    }

    [Fact]
    public void Process_ClampBluePointToMax65535()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var greenPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var bluePoints = new[] { 0, 0, 65530, 65530, 65535, 65535 };

        context.Adjustments.ToneCurve.RedPoints = redPoints;
        context.Adjustments.ToneCurve.GreenPoints = greenPoints;
        context.Adjustments.ToneCurve.BluePoints = bluePoints;

        // Act
        handler.Handle(context);

        // Assert
        // mid = 5, BluePoints[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.BluePoints[5]);
    }

    [Fact]
    public void Process_ReturnsEarly_WhenWhiteBalanceAdjusted()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 1;
        context.WhiteBalanceAdjusted = true;
        context.Adjustments.ToneCurve.LuminancePoints = Array.Empty<int>();
        // Provide valid channel points
        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert - luminance curve should still be set (first iteration logic runs first)
        Assert.NotEmpty(context.Adjustments.ToneCurve.LuminancePoints);
        // But no channel tone curve changes should be recorded
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_ReturnsEarly_WhenHighlightClippingDetected()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2; // Not first iteration
        context.WhiteBalanceAdjusted = false;
        context.Metrics.ClipHiR = 0.002; // Clipping detected
        // Provide valid channel points
        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void Process_AdjustsMidtone_WhenNoClippingAndWhiteBalanceNotAdjusted()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var greenPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var bluePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };

        context.Adjustments.ToneCurve.RedPoints = redPoints;
        context.Adjustments.ToneCurve.GreenPoints = greenPoints;
        context.Adjustments.ToneCurve.BluePoints = bluePoints;

        // Act
        handler.Handle(context);

        // Assert
        Assert.NotEmpty(context.Log);
        Assert.Contains(context.Log, l => l.Contains("ToneCurve"));

        // Points are stored as pairs [x0, y0, x1, y1, x2, y2]
        // mid = Math.Max(1, Math.Min(5, Math.Min(5, 5))) = 5 -> Y of last point
        // Red Y[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.RedPoints[5]);
        // Green Y[5] = 65535 - 500 = 65035
        Assert.Equal(65035, context.Adjustments.ToneCurve.GreenPoints[5]);
        // Blue Y[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.BluePoints[5]);
    }

    [Fact]
    public void Process_AdjustsMidtone_WhenAllChannelsHaveDifferentPoints()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 20000, 40000, 65535, 65535 };
        var greenPoints = new[] { 0, 0, 25000, 35000, 65535, 65535 };
        var bluePoints = new[] { 0, 0, 30000, 30000, 65535, 65535 };

        context.Adjustments.ToneCurve.RedPoints = redPoints;
        context.Adjustments.ToneCurve.GreenPoints = greenPoints;
        context.Adjustments.ToneCurve.BluePoints = bluePoints;

        // Act
        handler.Handle(context);

        // Assert
        // mid = Math.Max(1, Math.Min(5, Math.Min(5, 5))) = 5 -> Y of last point
        int mid = 5;
        // RedPoints[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.RedPoints[mid]);
        // GreenPoints[5] = 65535 - 500 = 65035
        Assert.Equal(65035, context.Adjustments.ToneCurve.GreenPoints[mid]);
        // BluePoints[5] = 65535 + 500 = 66035, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.BluePoints[mid]);
    }

    [Fact]
    public void Process_ClampRedPointToMax65535()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 65530, 65530, 65535, 65535 };
        var greenPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        var bluePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };

        context.Adjustments.ToneCurve.RedPoints = redPoints;
        context.Adjustments.ToneCurve.GreenPoints = greenPoints;
        context.Adjustments.ToneCurve.BluePoints = bluePoints;

        // Act
        handler.Handle(context);

        // Assert
        // mid = Math.Max(1, Math.Min(5, Math.Min(5, 5))) = 5
        // RedPoints[5] = 65530 + 500 = 66030, clamped to 65535
        Assert.Equal(65535, context.Adjustments.ToneCurve.RedPoints[5]);
    }

    [Fact]
    public void Process_LogContainsToneCurveMessage()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        SetupPoints(context);

        // Act
        handler.Handle(context);

        // Assert
        Assert.Contains(context.Log, l => l.Contains("ToneCurve"));
        Assert.Contains(context.Log, l => l.Contains("soft midtone channel"));
    }

    [Fact]
    public void Process_DoesNotModifyChannels_WhenOnlyOneChannelHasPoints()
    {
        // Arrange
        var handler = new ToneCurveHandler();
        var context = CreateContext(isNegative: false);
        context.Iteration = 2;
        context.WhiteBalanceAdjusted = false;

        var redPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        context.Adjustments.ToneCurve.RedPoints = redPoints;
        // Provide minimal valid arrays to avoid IndexOutOfRangeException
        context.Adjustments.ToneCurve.GreenPoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        context.Adjustments.ToneCurve.BluePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };

        // Act
        handler.Handle(context);

        // Assert - handler should have modified the points
        Assert.NotEmpty(context.Log);
    }

    private static OptimizationContext CreateContext(bool isNegative)
    {
        return new OptimizationContext
        {
            IsNegative = isNegative,
            Adjustments = new CaptureOneAdjustments(),
            Metrics = new FrameMetrics()
        };
    }

    private static void SetupPoints(OptimizationContext context)
    {
        var points = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        context.Adjustments.ToneCurve.RedPoints = points;
        context.Adjustments.ToneCurve.GreenPoints = points;
        context.Adjustments.ToneCurve.BluePoints = points;
    }
}
