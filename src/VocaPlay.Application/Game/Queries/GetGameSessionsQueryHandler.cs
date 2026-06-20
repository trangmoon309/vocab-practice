// VocaPlay.Application/Game/Queries/GetGameSessionsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;

namespace VocaPlay.Application.Game.Queries;

public class GetGameSessionsQueryHandler
{
    private readonly IGameSessionRepository _sessions;

    public GetGameSessionsQueryHandler(IGameSessionRepository sessions) => _sessions = sessions;

    /// <summary>Returns past game sessions for the current user.</summary>
    public async Task<IReadOnlyList<GameSessionDto>> Handle(GetGameSessionsQuery query, CancellationToken ct = default)
    {
        var sessions = await _sessions.GetByUserIdAsync(query.UserId, ct);
        return sessions.Select(s => new GameSessionDto(s.Id, s.Score, s.TotalPairs, s.CompletedAt)).ToList();
    }
}
