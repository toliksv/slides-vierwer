// SlidesViewer.Host/Rules/IMustApplyRule.cs

namespace SlidesViewer.Host.Rules;

/// <summary>
/// Правило, которое применяется после всех приоритетных правил,
/// но до fallback-правила. Гарантирует, что DefaultCategoryRule
/// не будет перехвачен приоритетными правилами.
/// </summary>
public interface IMustApplyRule
{
    RuleResultDto Apply(string input);
}
