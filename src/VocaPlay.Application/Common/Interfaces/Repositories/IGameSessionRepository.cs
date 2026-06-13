// VocaPlay.Application/Common/Interfaces/Repositories/IGameSessionRepository.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Common.Interfaces.Repositories;

/// <summary>Persistence contract for GameSession aggregate.</summary>
public interface IGameSessionRepository
{
    Task<IReadOnlyList<GameSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(GameSession session, CancellationToken ct = default);
}
