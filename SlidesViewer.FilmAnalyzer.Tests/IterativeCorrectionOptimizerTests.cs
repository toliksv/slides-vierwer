using NSubstitute;
using OpenCvSharp;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Optimizations;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;
using SlidesViewer.FilmAnalyzer.Services.Metrics;
using SlidesViewer.FilmAnalyzer.Services.Simulation;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class IterativeCorrectionOptimizerTests
{
    private readonly IOptimizationChainFactory _chainFactory;
    private readonly IPreviewSimulator _previewSimulator;
    private readonly IFrameMetricsComputer _metricsComputer;
    private readonly IterativeCorrectionOptimizer _optimizer;

    public IterativeCorrectionOptimizerTests()
    {
        _chainFactory = Substitute.For<IOptimizationChainFactory>();
        _previewSimulator = Substitute.For<IPreviewSimulator>();
        _metricsComputer = Substitute.For<IFrameMetricsComputer>();

        _optimizer = new IterativeCorrectionOptimizer(
            _chainFactory,
            _previewSimulator,
            _metricsComputer);
    }

    [Fact]
    public void Optimize_ReturnsNonNullAdjustments()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        SetupMocks(adjustments, isNegative);

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(log);
    }

    [Fact]
    public void Optimize_ReturnsSameAdjustmentsObject_WhenConvergedImmediately()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        SetupMocks(adjustments, isNegative, logMessages: Array.Empty<string>());

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.Same(adjustments, result);
    }

    [Fact]
    public void Optimize_ProducesIterationLog()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        SetupMocks(adjustments, isNegative, logMessages: new[] { "Iter 1: Changed" });

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.NotNull(log);
        Assert.NotEmpty(log);
    }

    [Fact]
    public void Optimize_StopsEarly_WhenConverged()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        // First iteration produces a log, second converges
        var callCount = 0;
        var handler = new CountingHandler(() => callCount++, ctx =>
        {
            if (callCount == 1)
                ctx.Log.Add("Iter 1: Changed");
            // Second call: no log = converged
        });

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert - should converge at iteration 2
        Assert.True(callCount <= 5, $"Handle should be called at most 5 times, was called {callCount}");
        Assert.Contains(log, l => l.Contains("Converged"));
    }

    [Fact]
    public void Optimize_HandlesIsNegative_True()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = true;

        SetupMocks(adjustments, isNegative);

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.NotNull(result);
        _metricsComputer.Received(1).Compute(Arg.Any<Mat>(), true);
    }

    [Fact]
    public void Optimize_HandlesIsNegative_False()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        SetupMocks(adjustments, isNegative);

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.NotNull(result);
        _metricsComputer.Received(1).Compute(Arg.Any<Mat>(), false);
    }

    [Fact]
    public void Optimize_CallsPreviewSimulatorForEachIteration()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        var handler = new ActionHandler(ctx => ctx.Log.Add("Changed"));

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert - should call Simulate up to 5 times (max iterations)
        _previewSimulator.Received(5).Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>());
    }

    [Fact]
    public void Optimize_CallsMetricsComputerForEachIteration()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        var handler = new ActionHandler(ctx => ctx.Log.Add("Changed"));

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        _metricsComputer.Received(5).Compute(Arg.Any<Mat>(), isNegative);
    }

    [Fact]
    public void Optimize_ContextIterationIsSetCorrectly()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        var iterationsSeen = new List<int>();

        var handler = new ActionHandler(ctx =>
        {
            iterationsSeen.Add(ctx.Iteration);
            if (ctx.Iteration < 3)
                ctx.Log.Add("Changed");
        });

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.Contains(1, iterationsSeen);
        Assert.Contains(2, iterationsSeen);
        Assert.Contains(3, iterationsSeen);
    }

    [Fact]
    public void Optimize_AdjustmentsAreModifiedByChain()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments
        {
            Exposure = new ExposureAdjustment { Exposure = 0 }
        };
        bool isNegative = false;

        var handler = new ActionHandler(ctx =>
        {
            ctx.Adjustments.Exposure.Exposure = 0.5;
            ctx.Log.Add("Changed");
        });

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        var result = _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.Equal(0.5, result.Exposure.Exposure);
    }

    [Fact]
    public void Optimize_LogContainsConvergedMessage_WhenNoChanges()
    {
        // Arrange
        var source = CreateTestMat(100, 100);
        var adjustments = new CaptureOneAdjustments();
        bool isNegative = false;

        var handler = new NoopHandler(); // Never adds to log = always converged

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());

        // Act
        _optimizer.Optimize(source, adjustments, isNegative, out var log);

        // Assert
        Assert.Contains(log, l => l.Contains("Converged"));
    }

    private static Mat CreateTestMat(int width, int height)
    {
        return new Mat(height, width, MatType.CV_8UC3, new Scalar(128, 128, 128));
    }

    private void SetupMocks(
        CaptureOneAdjustments adjustments,
        bool isNegative,
        string[] logMessages = null!)
    {
        var handler = new ActionHandler(ctx =>
        {
            if (logMessages != null)
            {
                foreach (var msg in logMessages)
                    ctx.Log.Add(msg);
            }
        });

        _chainFactory.Create().Returns(handler);
        _previewSimulator.Simulate(Arg.Any<Mat>(), Arg.Any<CaptureOneAdjustments>())
            .Returns(CreateTestMat(80, 80));
        _metricsComputer.Compute(Arg.Any<Mat>(), isNegative).Returns(new FrameMetrics());
    }

    /// <summary>
    /// Handler that performs an action on each handle call.
    /// </summary>
    private class ActionHandler : IOptimizationHandler
    {
        private readonly Action<OptimizationContext> _action;
        private IOptimizationHandler? _next;

        public ActionHandler(Action<OptimizationContext> action)
        {
            _action = action;
        }

        public int Order => 0;

        public void Handle(OptimizationContext context)
        {
            _action(context);
            _next?.Handle(context);
        }

        public IOptimizationHandler SetNext(IOptimizationHandler next)
        {
            _next = next;
            return next;
        }
    }

    /// <summary>
    /// Handler that counts calls and optionally modifies context.
    /// </summary>
    private class CountingHandler : IOptimizationHandler
    {
        private readonly Action _onCall;
        private readonly Action<OptimizationContext> _process;
        private IOptimizationHandler? _next;

        public CountingHandler(Action onCall, Action<OptimizationContext> process)
        {
            _onCall = onCall;
            _process = process;
        }

        public int Order => 0;

        public void Handle(OptimizationContext context)
        {
            _onCall();
            _process(context);
            _next?.Handle(context);
        }

        public IOptimizationHandler SetNext(IOptimizationHandler next)
        {
            _next = next;
            return next;
        }
    }

    /// <summary>
    /// Handler that does nothing (never logs = always converges).
    /// </summary>
    private class NoopHandler : IOptimizationHandler
    {
        private IOptimizationHandler? _next;

        public int Order => 0;

        public void Handle(OptimizationContext context)
        {
            _next?.Handle(context);
        }

        public IOptimizationHandler SetNext(IOptimizationHandler next)
        {
            _next = next;
            return next;
        }
    }
}
