// VocaPlay.Application/Words/DTOs/BulkAddResultDto.cs
namespace VocaPlay.Application.Words.DTOs;

public record BulkAddResultDto(int Added, int Skipped, IReadOnlyList<string> SkippedReasons);
