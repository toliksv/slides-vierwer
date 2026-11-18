using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Host.Repositories;
using SlidesViewer.Host.Rules;

namespace SlidesViewer.Host.Services;

/// <summary>
/// Реализация сервиса для работы с категориями слайдов.
/// </summary>
public class SlideService : ISlideService
{
    private readonly ISlideRepository _repository;
    private readonly IEnumerable<IRule> _priorityRules;
    private readonly IMustApplyRule _mustApplyRule;
    private readonly IFallbackRule _fallbackRule;

    public SlideService(
        ISlideRepository repository,
        IEnumerable<IRule> priorityRules,
        IMustApplyRule mustApplyRule,
        IFallbackRule fallbackRule)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _priorityRules = priorityRules ?? throw new ArgumentNullException(nameof(priorityRules));
        _mustApplyRule = mustApplyRule ?? throw new ArgumentNullException(nameof(mustApplyRule));
        _fallbackRule = fallbackRule ?? throw new ArgumentNullException(nameof(fallbackRule));
    }

    public async Task<GetCategoriesResponse> GetCategories(string folderPath)
    {
        var files = await _repository.SearchFilesAsync(folderPath);

        // Словарь: категория -> (количество файлов, набор уникальных относительных путей)
        var categoryData = new Dictionary<string, (int count, HashSet<string> paths)>();

        foreach (var file in files)
        {
            var category = GetCategory(file.FileName);

            if (!categoryData.TryGetValue(category, out var data))
            {
                categoryData[category] = (1, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { file.RelativePath });
            }
            else
            {
                categoryData[category] = (data.count + 1, data.paths);
                data.paths.Add(file.RelativePath);
            }
        }

        var categories = categoryData
            .OrderBy(x => x.Key)
            .Select(x => new SlideCategoryDto(
                x.Key,
                x.Value.count,
                [.. x.Value.paths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)]))
            .ToList();

        // Считаем счётчики
        var totalFiles = categories.Sum(c => c.FileCount);
        var totalCategories = categories.Count;
        var totalUniquePaths = categories.SelectMany(c => c.RelativePaths).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        return new GetCategoriesResponse
        {
            Categories = categories,
            TotalFiles = totalFiles,
            TotalCategories = totalCategories,
            TotalUniquePaths = totalUniquePaths
        };
    }

    private string GetCategory(string fileName)
    {
        // 1. Приоритетные правила (PCategoryRule, KCategoryRule)
        foreach (var rule in _priorityRules)
        {
            var result = rule.Apply(fileName);
            if (result.IsApplied && result.Value is not null)
            {
                return result.Value;
            }
        }

        // 2. Правило по умолчанию (DefaultCategoryRule)
        var mustResult = _mustApplyRule.Apply(fileName);
        if (mustResult.IsApplied && mustResult.Value is not null)
        {
            return mustResult.Value;
        }

        // 3. Fallback (NoCategoryRule)
        return _fallbackRule.Apply(fileName).Value ?? "Нет категории";
    }
}
