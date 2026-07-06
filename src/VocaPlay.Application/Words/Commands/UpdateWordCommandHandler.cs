// VocaPlay.Application/Words/Commands/UpdateWordCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Enums;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Words.Commands;

public class UpdateWordCommandHandler
{
    private readonly IWordRepository _words;

    public UpdateWordCommandHandler(IWordRepository words) => _words = words;

    /// <summary>Updates a word. Enforces ownership.</summary>
    public async Task<WordDto> Handle(UpdateWordCommand command, CancellationToken ct = default)
    {
        var word = await _words.GetByIdAsync(command.WordId, ct)
            ?? throw new NotFoundException(nameof(Word), command.WordId);

        if (word.UserId != command.UserId)
            throw new ForbiddenException();

        ValidateEnums(command.Level, command.Type);

        word.English = command.English.Trim();
        word.Vietnamese = command.Vietnamese.Trim();
        word.Pronunciation = command.Pronunciation?.Trim();
        word.Level = command.Level;
        word.Type = command.Type;
        word.ExampleSentence = command.ExampleSentence?.Trim();
        word.EnglishDefinition = command.EnglishDefinition?.Trim();
        word.UpdatedAt = DateTime.UtcNow;

        await _words.UpdateAsync(word, ct);
        return new WordDto(word.Id, word.English, word.Vietnamese, word.Pronunciation, word.Level, word.Type, word.ExampleSentence, word.EnglishDefinition);
    }

    private static void ValidateEnums(string? level, string? type)
    {
        if (level is not null && !Enum.TryParse<CefrLevel>(level, out _))
            throw new ValidationException($"Invalid level '{level}'. Valid values: A1, A2, B1, B2, C1, C2.");

        if (type is not null && !Enum.TryParse<WordType>(type, out _))
            throw new ValidationException($"Invalid type '{type}'.");
    }
}
