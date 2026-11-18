namespace SlidesViewer.Host.Repositories;

/// <summary>
/// Реализация репозитория для работы с файловой системой.
/// </summary>
public class FolderRepository : IFolderRepository
{
    public async Task<List<string>> GetSubdirectoriesAsync(string path)
    {
        return await Task.Run(() =>
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).ToList();
        }).ConfigureAwait(false);
    }
}
