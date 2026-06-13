// VocaPlay.Application/WordSets/DTOs/WordSetDto.cs
namespace VocaPlay.Application.WordSets.DTOs;

public record WordSetDto(Guid Id, string Title, string? Description, int WordCount, DateTime CreatedAt);
