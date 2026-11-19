namespace SlidesViewer.FilmAnalyzer.Optimization.Chains;

/// <summary>
/// Фабрика для создания цепочки обработчиков оптимизации.
/// </summary>
public interface IOptimizationChainFactory
{
    IOptimizationHandler Create();
}
