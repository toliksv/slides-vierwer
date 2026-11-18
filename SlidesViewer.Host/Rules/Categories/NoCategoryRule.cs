// SlidesViewer.Host/Rules/Categories/NoCategoryRule.cs

namespace SlidesViewer.Host.Rules.Categories;

internal class NoCategoryRule : IRule, IFallbackRule
{
    public RuleResultDto Apply(string input)
    {
        return new RuleResultDto("Нет категории", true);
    }
}
