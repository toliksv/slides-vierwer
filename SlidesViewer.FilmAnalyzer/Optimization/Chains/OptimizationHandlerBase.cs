using SlidesViewer.FilmAnalyzer.Optimization;

namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Базовое звено цепочки ответственности для обработки контекста оптимизации.
/// </summary>
public abstract class OptimizationHandlerBase : IOptimizationHandler
{
    private IOptimizationHandler? _next;

    public abstract int Order { get; }

    public IOptimizationHandler SetNext(IOptimizationHandler next)
    {
        _next = next;
        return next;
    }

    public void Handle(OptimizationContext context)
    {
        Process(context);
        _next?.Handle(context);
    }

    protected abstract void Process(OptimizationContext context);
}
