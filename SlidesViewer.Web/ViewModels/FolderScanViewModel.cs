using SlidesViewer.Host.Contracts.Categories;

namespace SlidesViewer.Web.ViewModels;

/// <summary>
/// Модель представления для страницы сканирования папки.
/// </summary>
public class FolderScanViewModel
{
    /// <summary>
    /// Путь по умолчанию, подставляемый в поле ввода.
    /// </summary>
    public string DefaultFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Путь к папке, введённый пользователем.
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Список категорий слайдов, полученных от API.
    /// </summary>
    public List<SlideCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// Общее количество найденных файлов (от API).
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Общее количество категорий (от API).
    /// </summary>
    public int TotalCategories { get; set; }

    /// <summary>
    /// Общее количество уникальных относительных путей (от API).
    /// </summary>
    public int TotalUniquePaths { get; set; }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Признак наличия результатов.
    /// </summary>
    public bool HasResults => Categories.Any();
}
