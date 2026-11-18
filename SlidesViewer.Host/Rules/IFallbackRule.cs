// SlidesViewer.Host/Rules/IFallbackRule.cs

namespace SlidesViewer.Host.Rules;

/// <summary>
/// Маркер интерфейса для правила-заглушки, которое применяется,
/// когда ни одно из приоритетных правил не сработало.
/// </summary>
public interface IFallbackRule
{
    /// <summary>
    /// Применяет правило-заглушку к входному тексту.
    /// </summary>
    RuleResultDto Apply(string input);
}
