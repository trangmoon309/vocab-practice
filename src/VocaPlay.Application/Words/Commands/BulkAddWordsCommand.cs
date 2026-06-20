// VocaPlay.Application/Words/Commands/BulkAddWordsCommand.cs
namespace VocaPlay.Application.Words.Commands;

public record WordInput(
    string English,
    string Vietnamese,
    string? Pronunciation,
    string? Level,
    string? Type,
    string? ExampleSentence);

public record BulkAddWordsCommand(Guid UserId, IReadOnlyList<WordInput> Words);
