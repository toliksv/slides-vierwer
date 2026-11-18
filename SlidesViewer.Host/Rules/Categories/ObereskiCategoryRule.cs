// SlidesViewer.Host/Rules/Categories/ObereskiCategoryRule.cs

namespace SlidesViewer.Host.Rules.Categories;

internal class ObereskiCategoryRule : IRule
{
    public RuleResultDto Apply(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new RuleResultDto(null, false);

        int index = input.IndexOf("_обрезки", StringComparison.OrdinalIgnoreCase);
        if (index == -1)
        {
            return new RuleResultDto(null, false);
        }

        return new RuleResultDto(input[..index], true);
    }
}
