using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Optimization;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class OptimizationContextTests
{
    [Fact]
    public void Adjustments_DefaultsToNull()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.Null(context.Adjustments);
    }

    [Fact]
    public void Metrics_DefaultsToEmpty()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.NotNull(context.Metrics);
    }

    [Fact]
    public void IsNegative_DefaultsToFalse()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.False(context.IsNegative);
    }

    [Fact]
    public void Iteration_DefaultsToZero()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.Equal(0, context.Iteration);
    }

    [Fact]
    public void ShadowAdjusted_DefaultsToFalse()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.False(context.ShadowAdjusted);
    }

    [Fact]
    public void WhiteBalanceAdjusted_DefaultsToFalse()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.False(context.WhiteBalanceAdjusted);
    }

    [Fact]
    public void ModifiedKeys_DefaultsToEmpty()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.Empty(context.ModifiedKeys);
    }

    [Fact]
    public void Log_DefaultsToEmpty()
    {
        // Arrange & Act
        var context = new OptimizationContext();

        // Assert
        Assert.Empty(context.Log);
    }

    [Fact]
    public void RecordChange_AddsKeyToModifiedKeys()
    {
        // Arrange
        var context = new OptimizationContext();
        context.Iteration = 1;

        // Act
        context.RecordChange("Exposure", "+0.10 EV");

        // Assert
        Assert.Contains(context.ModifiedKeys, k => k == "Exposure");
    }

    [Fact]
    public void RecordChange_AddsFormattedEntryToLog()
    {
        // Arrange
        var context = new OptimizationContext();
        context.Iteration = 3;

        // Act
        context.RecordChange("WhiteBalance", "CT=5600K");

        // Assert
        Assert.Contains(context.Log, l => l == "Iter 3 [WhiteBalance]: CT=5600K");
    }

    [Fact]
    public void RecordChange_LogContainsIterationKeyAndMessage()
    {
        // Arrange
        var context = new OptimizationContext();
        context.Iteration = 5;

        // Act
        context.RecordChange("Levels", "S=[0.05,0.05,0.05] H=[0.95,0.95,0.95]");

        // Assert
        Assert.Contains(context.Log, l => l.Contains("Iter 5"));
        Assert.Contains(context.Log, l => l.Contains("Levels"));
        Assert.Contains(context.Log, l => l.Contains("S=["));
        Assert.Contains(context.Log, l => l.Contains("H=["));
    }

    [Fact]
    public void RecordChange_CanRecordMultipleChanges()
    {
        // Arrange
        var context = new OptimizationContext();
        context.Iteration = 1;

        // Act
        context.RecordChange("Levels", "first");
        context.RecordChange("Exposure", "second");
        context.RecordChange("WhiteBalance", "third");

        // Assert
        Assert.Equal(3, context.ModifiedKeys.Count);
        Assert.Equal(3, context.Log.Count);
        Assert.Contains(context.ModifiedKeys, k => k == "Levels");
        Assert.Contains(context.ModifiedKeys, k => k == "Exposure");
        Assert.Contains(context.ModifiedKeys, k => k == "WhiteBalance");
    }

    [Fact]
    public void RecordChange_CanRecordSameKeyMultipleTimes()
    {
        // Arrange
        var context = new OptimizationContext();
        context.Iteration = 1;

        // Act
        context.RecordChange("Exposure", "first change");
        context.RecordChange("Exposure", "second change");

        // Assert - ModifiedKeys is a HashSet, so it should have only one entry
        Assert.Single(context.ModifiedKeys);
        Assert.Equal(2, context.Log.Count); // But Log is a List, so both entries
    }

    [Fact]
    public void Adjustments_CanBeSet()
    {
        // Arrange
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment { Exposure = 0.5 }
        };

        // Act
        var context = new OptimizationContext { Adjustments = adjustments };

        // Assert
        Assert.Same(adjustments, context.Adjustments);
        Assert.Equal(0.5, context.Adjustments.Exposure.Exposure);
    }

    [Fact]
    public void Metrics_CanBeSet()
    {
        // Arrange
        var context = new OptimizationContext();
        var metrics = new Dtos.Optimizations.FrameMetrics
        {
            P50_R = 0.5,
            P50_G = 0.5,
            P50_B = 0.5
        };

        // Act
        context.Metrics = metrics;

        // Assert
        Assert.Same(metrics, context.Metrics);
        Assert.Equal(0.5, context.Metrics.P50_R);
    }

    [Fact]
    public void IsNegative_CanBeSet()
    {
        // Arrange
        var context = new OptimizationContext { IsNegative = true };

        // Act & Assert
        Assert.True(context.IsNegative);
    }

    [Fact]
    public void Iteration_CanBeSet()
    {
        // Arrange
        var context = new OptimizationContext();

        // Act
        context.Iteration = 7;

        // Assert
        Assert.Equal(7, context.Iteration);
    }

    [Fact]
    public void ShadowAdjusted_CanBeSet()
    {
        // Arrange
        var context = new OptimizationContext();

        // Act
        context.ShadowAdjusted = true;

        // Assert
        Assert.True(context.ShadowAdjusted);
    }

    [Fact]
    public void WhiteBalanceAdjusted_CanBeSet()
    {
        // Arrange
        var context = new OptimizationContext();

        // Act
        context.WhiteBalanceAdjusted = true;

        // Assert
        Assert.True(context.WhiteBalanceAdjusted);
    }
}
