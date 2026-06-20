// VocaPlay.Application/Words/Commands/AddWordCommand.cs
namespace VocaPlay.Application.Words.Commands;

public record AddWordCommand(
    Guid UserId,
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence);
