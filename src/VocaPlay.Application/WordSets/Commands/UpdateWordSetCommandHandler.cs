// VocaPlay.Application/WordSets/Commands/UpdateWordSetCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.WordSets.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.WordSets.Commands;

public class UpdateWordSetCommandHandler
{
    private readonly IWordSetRepository _wordSets;

    public UpdateWordSetCommandHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Updates title and description of a word set. Enforces ownership.</summary>
    public async Task<WordSetDto> Handle(UpdateWordSetCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        set.Title = command.Title.Trim();
        set.Description = command.Description?.Trim();
        set.UpdatedAt = DateTime.UtcNow;

        await _wordSets.UpdateAsync(set, ct);
        return new WordSetDto(set.Id, set.Title, set.Description, set.Words.Count, set.CreatedAt);
    }
}
