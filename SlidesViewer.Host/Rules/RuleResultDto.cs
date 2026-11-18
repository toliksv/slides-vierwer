// SlidesViewer.Host/Rules/DTOs/RuleResultDto.cs
namespace SlidesViewer.Host.Rules;

/// <summary>
/// DTO для представления результата применения правила.
/// </summary>
public record RuleResultDto(string? Value, bool IsApplied);
