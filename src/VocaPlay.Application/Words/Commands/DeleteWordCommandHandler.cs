// VocaPlay.Application/Words/Commands/DeleteWordCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Words.Commands;

public class DeleteWordCommandHandler
{
    private readonly IWordSetRepository _wordSets;
    private readonly IWordRepository _words;

    public DeleteWordCommandHandler(IWordSetRepository wordSets, IWordRepository words)
    {
        _wordSets = wordSets;
        _words = words;
    }

    /// <summary>Deletes a word after verifying ownership chain: user → word set → word.</summary>
    public async Task Handle(DeleteWordCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        var word = await _words.GetByIdAsync(command.WordId, ct)
            ?? throw new NotFoundException(nameof(Word), command.WordId);

        if (word.WordSetId != command.WordSetId)
            throw new ForbiddenException();

        await _words.DeleteAsync(word, ct);
    }
}
