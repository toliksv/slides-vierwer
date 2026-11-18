using RestEase;
using SlidesViewer.Host.Contracts.Categories;

namespace SlidesViewer.Host.Contracts.Api;

/// <summary>
/// REST-клиент для Slides API.
/// </summary>
public interface ISlidesApi
{
    /// <summary>
    /// Получить список категорий слайдов по пути к папке.
    /// </summary>
    [Post("/api/slides/categories")]
    Task<GetCategoriesResponse> GetCategories([Body] GetCategoriesRequest request);

    /// <summary>
    /// Получить список подпапок по префиксу пути (autocomplete).
    /// </summary>
    [Get("/api/slides/folders")]
    Task<List<string>> GetFolders([Query] string prefix);
}
