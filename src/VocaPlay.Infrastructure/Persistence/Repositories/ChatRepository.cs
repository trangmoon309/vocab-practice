// VocaPlay.Infrastructure/Persistence/Repositories/ChatRepository.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Persistence;

namespace VocaPlay.Infrastructure.Persistence.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _db;

    public ChatRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ChatMessage>> GetByUserIdAsync(Guid userId, int limit, CancellationToken ct = default)
        => await _db.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)   // oldest-first for AI sliding window context
            .ToListAsync(ct);

    public async Task AddAsync(ChatMessage message, CancellationToken ct = default)
    {
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default)
    {
        _db.ChatMessages.AddRange(messages);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.ChatMessages.Where(m => m.UserId == userId).ExecuteDeleteAsync(ct);
    }
}
