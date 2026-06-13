// VocaPlay.Application/WordSets/Commands/DeleteWordSetCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.WordSets.Commands;

public class DeleteWordSetCommandHandler
{
    private readonly IWordSetRepository _wordSets;

    public DeleteWordSetCommandHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Deletes a word set and all its words (cascade). Enforces ownership.</summary>
    public async Task Handle(DeleteWordSetCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        await _wordSets.DeleteAsync(set, ct);
    }
}
