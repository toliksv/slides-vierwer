using NSubstitute;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Optimization;
using SlidesViewer.FilmAnalyzer.Optimization.Chains;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class OptimizationChainFactoryTests
{
    [Fact]
    public void Create_ThrowsException_WhenNoHandlersRegistered()
    {
        // Arrange
        var factory = new OptimizationChainFactory();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory.Create());
        Assert.Contains("No optimization handlers registered", exception.Message);
    }

    [Fact]
    public void Create_ReturnsHandler_WhenOneHandlerRegistered()
    {
        // Arrange
        var handler = new TestHandler(1);
        var factory = new OptimizationChainFactory(new[] { handler });

        // Act
        var result = factory.Create();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IOptimizationHandler>(result);
        Assert.Same(handler, result);
    }

    [Fact]
    public void Create_ChainsHandlers_InOrder()
    {
        // Arrange
        var handler1 = new TestHandler(1);
        var handler2 = new TestHandler(2);
        var handler3 = new TestHandler(3);
        var factory = new OptimizationChainFactory(new[] { handler3, handler1, handler2 });

        // Act
        var result = factory.Create();

        // Assert - first handler should be the one with Order=1
        Assert.Same(handler1, result);

        // Verify chain: handler1 -> handler2 -> handler3
        // We can't directly verify the _next field, but we can test via Handle
        var context = new OptimizationContext { Adjustments = new CaptureOneAdjustments() };

        ((IOptimizationHandler)result).Handle(context);

        // All handlers should have been called (they all record changes)
        Assert.Contains(context.Log, l => l.Contains("Handler1"));
        Assert.Contains(context.Log, l => l.Contains("Handler2"));
        Assert.Contains(context.Log, l => l.Contains("Handler3"));
    }

    [Fact]
    public void Create_ChainsHandlers_WithUnorderedRegistration()
    {
        // Arrange - register in reverse order
        IOptimizationHandler[] handlers = new IOptimizationHandler[]
        {
            new TestHandler(5),
            new TestHandler(1),
            new TestHandler(3),
            new TestHandler(2),
            new TestHandler(4)
        };
        var factory = new OptimizationChainFactory(handlers);

        // Act
        var result = factory.Create();

        // Assert - first should be Order=1
        Assert.Same(handlers[1], result); // handler with Order=1
    }

    [Fact]
    public void Handle_ExecutesAllHandlersInChain()
    {
        // Arrange
        var handler1 = new TestHandler(1);
        var handler2 = new TestHandler(2);
        var factory = new OptimizationChainFactory(new[] { handler1, handler2 });

        var context = new OptimizationContext { Adjustments = new CaptureOneAdjustments() };

        // Act
        var chain = factory.Create();
        ((IOptimizationHandler)chain).Handle(context);

        // Assert
        Assert.Equal(2, context.Log.Count);
        Assert.Contains(context.Log, l => l.Contains("Handler1"));
        Assert.Contains(context.Log, l => l.Contains("Handler2"));
    }

    [Fact]
    public void Handle_StopsWhenHandlerDoesNotAddLog()
    {
        // Arrange
        var handler1 = new TestHandler(1);
        var handler2 = new NoLogHandler(2); // Never adds to log
        IOptimizationHandler[] handlers = new IOptimizationHandler[2] { handler1, handler2 };
        var factory = new OptimizationChainFactory(handlers);

        var context = new OptimizationContext { Adjustments = new CaptureOneAdjustments() };
        context.Iteration = 1;

        // Act
        var chain = factory.Create();
        ((IOptimizationHandler)chain).Handle(context);

        // Assert - handler1 adds, handler2 (NoLogHandler) doesn't add anything
        Assert.Contains(context.Log, l => l.Contains("Handler1"));
        // NoLogHandler intentionally doesn't add to log
        Assert.DoesNotContain(context.Log, l => l.Contains("Handler2"));
    }

    [Fact]
    public void SetNext_ReturnsNextHandler()
    {
        // Arrange
        var handler1 = new TestHandler(1);
        var handler2 = new TestHandler(2);

        // Act
        var returned = handler1.SetNext(handler2);

        // Assert
        Assert.Same(handler2, returned);
    }

    [Fact]
    public void SetNext_ChainsHandlers()
    {
        // Arrange
        var handler1 = new TestHandler(1);
        var handler2 = new TestHandler(2);
        var handler3 = new TestHandler(3);

        // Act - manually chain
        handler1.SetNext(handler2).SetNext(handler3);

        var context = new OptimizationContext { Adjustments = new CaptureOneAdjustments() };

        handler1.Handle(context);

        // Assert
        Assert.Contains(context.Log, l => l.Contains("Handler1"));
        Assert.Contains(context.Log, l => l.Contains("Handler2"));
        Assert.Contains(context.Log, l => l.Contains("Handler3"));
    }

    /// <summary>
    /// Test handler that always records a change.
    /// </summary>
    private class TestHandler : OptimizationHandlerBase
    {
        private readonly int _order;
        private readonly string _name;

        public TestHandler(int order, string name = null!)
        {
            _order = order;
            _name = name ?? $"Handler{order}";
        }

        public override int Order => _order;

        protected override void Process(Optimization.OptimizationContext context)
        {
            context.RecordChange(_name, $"Processed by {_name}");
        }
    }

    /// <summary>
    /// Test handler that never records a change.
    /// </summary>
    private class NoLogHandler : OptimizationHandlerBase
    {
        private readonly int _order;

        public NoLogHandler(int order)
        {
            _order = order;
        }

        public override int Order => _order;

        protected override void Process(Optimization.OptimizationContext context)
        {
            // Intentionally does nothing - no log entry
        }
    }
}
