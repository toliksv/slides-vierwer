// SlidesViewer.Host/Rules/Categories/DefaultCategoryRule.cs

namespace SlidesViewer.Host.Rules.Categories;

internal class DefaultCategoryRule : IRule, IMustApplyRule
{
    public RuleResultDto Apply(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new RuleResultDto(null, false);
        int lastUnderscoreIndex = input.LastIndexOf('_');
        if (lastUnderscoreIndex == -1)
        {
            // Если не найден символ "_", вернуть пустое значение с признаком IsApplied равным false
            return new RuleResultDto(null, false);
        }

        return new RuleResultDto(input[..lastUnderscoreIndex], true);
    }
}
