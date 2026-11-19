namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Звено цепочки обработки оптимизации.
/// </summary>
public interface IOptimizationHandler
{
    int Order { get; }

    void Handle(OptimizationContext context);

    IOptimizationHandler SetNext(IOptimizationHandler next);
}
