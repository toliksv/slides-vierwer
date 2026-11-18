namespace SlidesViewer.Host.Repositories;

/// <summary>
/// Репозиторий для работы с файловой системой.
/// </summary>
public interface IFolderRepository
{
    /// <summary>
    /// Получает список подпапок в указанной директории.
    /// </summary>
    /// <param name="path">Путь к директории.</param>
    /// <returns>Список полных путей к подпапкам.</returns>
    Task<List<string>> GetSubdirectoriesAsync(string path);
}
