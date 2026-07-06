// VocaPlay.Application/Words/DTOs/WordDto.cs
namespace VocaPlay.Application.Words.DTOs;

public record WordDto(
    Guid Id,
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence,
    string? EnglishDefinition);
