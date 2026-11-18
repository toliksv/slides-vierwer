namespace SlidesViewer.Host.Rules;

/// <summary>
/// Интерфейс для правила, которое применяется к входному тексту.
/// </summary>
public interface IRule
{
    /// <summary>
    /// Применяет правило к входному тексту и возвращает результат.
    /// </summary>
    /// <param name="input">Входной текст.</param>
    /// <returns>Результат применения правила.</returns>
    RuleResultDto Apply(string input);
}