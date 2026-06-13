// VocaPlay.Application/WordSets/DTOs/WordSetDetailDto.cs
using VocaPlay.Application.Words.DTOs;

namespace VocaPlay.Application.WordSets.DTOs;

public record WordSetDetailDto(
    Guid Id,
    string Title,
    string? Description,
    int WordCount,
    IReadOnlyList<WordDto> Words,
    DateTime CreatedAt);
