namespace SlidesViewer.Host.Repositories;

internal class SlideRepository : ISlideRepository
{
    public async Task<List<SlideFileEntity>> SearchFilesAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(folderPath))
            return [];

        try
        {
            var slideFiles = await Task.Run(() =>
            {
                var files = Directory.GetFiles(folderPath, "*.tiff", SearchOption.AllDirectories);                
                var result = new List<SlideFileEntity>(files.Length);

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileInfo = new FileInfo(file);
                    result.Add(new SlideFileEntity
                    {
                        FileName = fileInfo.Name,
                        RelativePath = Path.GetRelativePath(folderPath, Path.GetDirectoryName(file)!),
                        Size = fileInfo.Length
                    });
                }

                return result;
            }, cancellationToken).ConfigureAwait(false);

            return slideFiles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching files: {ex.Message}");
            return [];
        }
    }
}