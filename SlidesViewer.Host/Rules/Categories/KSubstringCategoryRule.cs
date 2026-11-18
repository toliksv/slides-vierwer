// SlidesViewer.Host/Rules/Categories/KSubstringCategoryRule.cs

namespace SlidesViewer.Host.Rules.Categories;

internal class KSubstringCategoryRule : IRule
{
    public RuleResultDto Apply(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new RuleResultDto(null, false);

        int index = input.IndexOf("_k", StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return new RuleResultDto(null, false);
        }

        return new RuleResultDto(input[..index], true);
    }
}
