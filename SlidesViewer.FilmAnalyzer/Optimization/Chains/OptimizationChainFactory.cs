namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Реализация фабрики, собирающая цепочку из зарегистрированных обработчиков по порядку Order.
/// </summary>
public class OptimizationChainFactory : IOptimizationChainFactory
{
    private readonly IEnumerable<IOptimizationHandler> _handlers;

    public OptimizationChainFactory(IEnumerable<IOptimizationHandler>? handlers = null)
    {
        _handlers = handlers ?? Array.Empty<IOptimizationHandler>();
    }

    public IOptimizationHandler Create()
    {
        var ordered = _handlers.OrderBy(h => h.Order).ToList();

        if (ordered.Count == 0)
            throw new InvalidOperationException("No optimization handlers registered.");

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            ordered[i].SetNext(ordered[i + 1]);
        }

        return ordered.First();
    }
}
