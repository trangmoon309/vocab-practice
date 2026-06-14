// VocaPlay.Infrastructure/Persistence/Repositories/GameSessionRepository.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Persistence;

namespace VocaPlay.Infrastructure.Persistence.Repositories;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly AppDbContext _db;

    public GameSessionRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<GameSession>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.GameSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync(ct);

    public async Task AddAsync(GameSession session, CancellationToken ct = default)
    {
        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync(ct);
    }
}
