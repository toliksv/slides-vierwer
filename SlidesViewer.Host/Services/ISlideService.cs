using SlidesViewer.Host.Contracts.Categories;

namespace SlidesViewer.Host.Services;

/// <summary>
/// Интерфейс для работы с категориями слайдов.
/// </summary>
public interface ISlideService
{
    /// <summary>
    /// Получает список категорий слайдов для указанной папки.
    /// </summary>
    /// <param name="folderPath">Путь к папке, содержащей слайды.</param>
    /// <returns>Ответ с DTO-объектами категорий и счётчиками.</returns>
    Task<GetCategoriesResponse> GetCategories(string folderPath);
}
