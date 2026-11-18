namespace SlidesViewer.Host.Services;

/// <summary>
/// Сервис для работы с папками (автодополнение путей).
/// </summary>
public interface IFolderService
{
    /// <summary>
    /// Получает список доступных подпапок по заданному пути.
    /// Исключает системные, скрытые и защищённые папки.
    /// </summary>
    /// <param name="path">Путь к директории.</param>
    /// <returns>Список путей к доступным подпапкам, отсортированный по алфавиту.</returns>
    Task<List<string>> GetAccessibleFoldersAsync(string path);
}
