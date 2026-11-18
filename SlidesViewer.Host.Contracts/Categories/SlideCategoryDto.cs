using System.Text.Json.Serialization;

namespace SlidesViewer.Host.Contracts.Categories;

public class SlideCategoryDto
{
    [JsonPropertyOrder(0)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    public int FileCount { get; set; }

    [JsonPropertyOrder(2)]
    public List<string> RelativePaths { get; set; } = new();

    public SlideCategoryDto()
    {
    }

    [JsonConstructor]
    public SlideCategoryDto(string Name, int FileCount, List<string> RelativePaths)
    {
        this.Name = Name;
        this.FileCount = FileCount;
        this.RelativePaths = RelativePaths ?? new();
    }

    public SlideCategoryDto(string name, int fileCount)
        : this(name, fileCount, new())
    {
    }
}