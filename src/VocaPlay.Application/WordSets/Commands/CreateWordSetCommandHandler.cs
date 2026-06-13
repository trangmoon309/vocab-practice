// VocaPlay.Application/WordSets/Commands/CreateWordSetCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.WordSets.DTOs;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.WordSets.Commands;

public class CreateWordSetCommandHandler
{
    private readonly IWordSetRepository _wordSets;

    public CreateWordSetCommandHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Creates a new word set for the current user.</summary>
    public async Task<WordSetDto> Handle(CreateWordSetCommand command, CancellationToken ct = default)
    {
        var set = new WordSet
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Title = command.Title.Trim(),
            Description = command.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _wordSets.AddAsync(set, ct);
        return new WordSetDto(set.Id, set.Title, set.Description, 0, set.CreatedAt);
    }
}
