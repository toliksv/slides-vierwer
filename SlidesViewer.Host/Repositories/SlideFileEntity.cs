using System;

namespace SlidesViewer.Host.Repositories;

/// <summary>
/// Данные о файле слайда.
/// </summary>
public record SlideFileEntity
{
    /// <summary>
    /// Имя файла.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Относительный путь к файлу.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Размер файла в байтах.
    /// </summary>
    public long Size { get; init; }
}