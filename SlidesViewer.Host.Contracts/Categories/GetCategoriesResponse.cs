namespace SlidesViewer.Host.Contracts.Categories;

/// <summary>
/// Ответ на запрос категорий слайдов.
/// </summary>
public class GetCategoriesResponse
{
    /// <summary>
    /// Список категорий слайдов.
    /// </summary>
    public List<SlideCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// Общее количество найденных файлов.
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Общее количество категорий.
    /// </summary>
    public int TotalCategories { get; set; }

    /// <summary>
    /// Общее количество уникальных относительных путей.
    /// </summary>
    public int TotalUniquePaths { get; set; }
}
