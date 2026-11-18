using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlidesViewer.Host.Repositories;

public interface ISlideRepository
{
    Task<List<SlideFileEntity>> SearchFilesAsync(string folderPath, CancellationToken cancellationToken = default);
}