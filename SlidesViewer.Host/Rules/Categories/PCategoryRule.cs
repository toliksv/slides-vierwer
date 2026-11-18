// MyApp.Core/Rules/Categories/PCategoryRule.cs

using Microsoft.AspNetCore.Rewrite;
namespace SlidesViewer.Host.Rules.Categories;

internal class PCategoryRule : IRule
{
    public RuleResultDto Apply(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new RuleResultDto(null, false);
        int firstUnderscoreIndex = input.IndexOf("_п", StringComparison.OrdinalIgnoreCase);
        if (firstUnderscoreIndex == -1)
        {
            // Если не найдена подстрока "_п", вернуть пустое значение с признаком IsApplied равным false
            return new RuleResultDto(null, false);
        }

        return new RuleResultDto(input[..firstUnderscoreIndex], true);
    }
}

