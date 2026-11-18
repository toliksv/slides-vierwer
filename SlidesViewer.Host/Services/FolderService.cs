using SlidesViewer.Host.Repositories;

namespace SlidesViewer.Host.Services;

/// <summary>
/// Системные/защищённые папки Windows, которые нужно исключать из списка.
/// </summary>
internal static class SystemFolders
{
    public static readonly HashSet<string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        "Recycler", "RECYCLED", "Recycled",
        "System Volume Information",
        "$RECYCLE.BIN",
        "ProgramData", "Program Files", "Program Files (x86)",
        "Windows", "WinSxS",
        "PerfLogs", "Boot",
        "$NTFS_JOURNAL",
        "WindowsApps",
        "Documents and Settings"
    };
}

/// <summary>
/// Реализация сервиса для работы с папками.
/// </summary>
public class FolderService : IFolderService
{
    private readonly IFolderRepository _repository;

    public FolderService(IFolderRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<string>> GetAccessibleFoldersAsync(string path)
    {
        var normalizedPath = NormalizePath(path);

        if (!Directory.Exists(normalizedPath))
            return [];

        var subdirs = await _repository.GetSubdirectoriesAsync(normalizedPath).ConfigureAwait(false);

        var result = new List<string>();

        foreach (var dir in subdirs)
        {
            if (IsAccessibleFolder(dir))
            {
                result.Add(dir + "\\");
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Нормализует путь: убирает trailing slashes, добавляет backslash для диска.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        var normalized = path.TrimEnd('\\', '/');

        // Если путь заканчивается на букву диска с двоеточием (C:), добавляем разделитель
        if (normalized.Length >= 2 && normalized[1] == ':' && normalized.Length == 2)
        {
            normalized += "\\";
        }

        return normalized;
    }

    /// <summary>
    /// Проверяет, является ли папка доступной (не системной, не скрытой).
    /// </summary>
    private static bool IsAccessibleFolder(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);

            // Пропускаем скрытые папки
            if ((dirInfo.Attributes & FileAttributes.Hidden) != 0)
                return false;

            // Пропускаем системные папки
            if ((dirInfo.Attributes & FileAttributes.System) != 0)
                return false;

            // Пропускаем папки с атрибутами повторной индексации (symlinks, junctions)
            if ((dirInfo.Attributes & FileAttributes.ReparsePoint) != 0)
                return false;

            // Пропускаем известные системные папки по имени
            if (SystemFolders.Names.Contains(dirInfo.Name))
                return false;

            return true;
        }
        catch
        {
            // Если нет доступа к папке — пропускаем
            return false;
        }
    }
}
