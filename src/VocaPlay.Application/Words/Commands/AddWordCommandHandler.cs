// VocaPlay.Application/Words/Commands/AddWordCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Enums;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Words.Commands;

public class AddWordCommandHandler
{
    private readonly IWordSetRepository _wordSets;
    private readonly IWordRepository _words;

    public AddWordCommandHandler(IWordSetRepository wordSets, IWordRepository words)
    {
        _wordSets = wordSets;
        _words = words;
    }

    /// <summary>Adds a single word to a word set. Enforces ownership and validates enum values.</summary>
    public async Task<WordDto> Handle(AddWordCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        ValidateEnums(command.Level, command.Type);

        var word = new Word
        {
            Id = Guid.NewGuid(),
            WordSetId = command.WordSetId,
            English = command.English.Trim(),
            Vietnamese = command.Vietnamese.Trim(),
            Pronunciation = command.Pronunciation?.Trim(),
            Level = command.Level,
            Type = command.Type,
            ExampleSentence = command.ExampleSentence?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _words.AddAsync(word, ct);
        return new WordDto(word.Id, word.English, word.Vietnamese, word.Pronunciation, word.Level, word.Type, word.ExampleSentence);
    }

    private static void ValidateEnums(string? level, string? type)
    {
        if (level is not null && !Enum.TryParse<CefrLevel>(level, out _))
            throw new ValidationException($"Invalid level '{level}'. Valid values: A1, A2, B1, B2, C1, C2.");

        if (type is not null && !Enum.TryParse<WordType>(type, out _))
            throw new ValidationException($"Invalid type '{type}'. Valid values: Noun, Verb, Adjective, Adverb, Preposition, Conjunction, Pronoun, Interjection.");
    }
}
