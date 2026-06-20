// VocaPlay.Application/Words/Commands/UpdateWordCommand.cs
namespace VocaPlay.Application.Words.Commands;

public record UpdateWordCommand(
    Guid WordId,
    Guid UserId,
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence);
