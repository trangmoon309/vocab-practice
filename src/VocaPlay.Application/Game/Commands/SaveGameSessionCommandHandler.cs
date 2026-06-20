// VocaPlay.Application/Game/Commands/SaveGameSessionCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Game.Commands;

public class SaveGameSessionCommandHandler
{
    private readonly IGameSessionRepository _sessions;

    public SaveGameSessionCommandHandler(IGameSessionRepository sessions) => _sessions = sessions;

    /// <summary>Persists a completed game session.</summary>
    public async Task<GameSessionDto> Handle(SaveGameSessionCommand command, CancellationToken ct = default)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Score = command.Score,
            TotalPairs = command.TotalPairs,
            CompletedAt = DateTime.UtcNow
        };

        await _sessions.AddAsync(session, ct);
        return new GameSessionDto(session.Id, session.Score, session.TotalPairs, session.CompletedAt);
    }
}
