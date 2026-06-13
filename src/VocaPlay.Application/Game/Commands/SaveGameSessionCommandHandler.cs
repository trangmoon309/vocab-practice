// VocaPlay.Application/Game/Commands/SaveGameSessionCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Game.Commands;

public class SaveGameSessionCommandHandler
{
    private readonly IWordSetRepository _wordSets;
    private readonly IGameSessionRepository _sessions;

    public SaveGameSessionCommandHandler(IWordSetRepository wordSets, IGameSessionRepository sessions)
    {
        _wordSets = wordSets;
        _sessions = sessions;
    }

    /// <summary>Persists a completed game session after verifying the word set exists.</summary>
    public async Task<GameSessionDto> Handle(SaveGameSessionCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            WordSetId = command.WordSetId,
            Score = command.Score,
            TotalPairs = command.TotalPairs,
            CompletedAt = DateTime.UtcNow
        };

        await _sessions.AddAsync(session, ct);
        return new GameSessionDto(session.Id, session.WordSetId, session.Score, session.TotalPairs, session.CompletedAt);
    }
}
