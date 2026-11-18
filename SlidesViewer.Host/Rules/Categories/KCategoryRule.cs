// SlidesViewer.Host/Rules/Categories/KCategoryRule.cs

namespace SlidesViewer.Host.Rules.Categories;

internal class KCategoryRule : IRule
{
    private readonly IRule _defaultCategoryRule;

    public KCategoryRule([FromKeyedServices("DefaultCategory")]IRule defaultCategoryRule)
    {
        _defaultCategoryRule = defaultCategoryRule ?? throw new ArgumentNullException(nameof(defaultCategoryRule));
    }

    public RuleResultDto Apply(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new RuleResultDto(null, false);
        int firstUnderscoreIndex = input.IndexOf("_к", StringComparison.OrdinalIgnoreCase);
        if (firstUnderscoreIndex == -1)
        {
            // Если не найдена подстрока "_к", вернуть пустое значение с признаком IsApplied равным false
            return new RuleResultDto(null, false);
        }

        return new RuleResultDto(input[..firstUnderscoreIndex], true);
    }
}