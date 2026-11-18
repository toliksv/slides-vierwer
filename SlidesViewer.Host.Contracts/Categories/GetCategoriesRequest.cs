namespace SlidesViewer.Host.Contracts.Categories;

/// <summary>
/// Запрос на получение категорий слайдов.
/// </summary>
public class GetCategoriesRequest
{
    /// <summary>
    /// Путь к папке, содержащей слайды.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;
}
