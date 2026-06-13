// VocaPlay.Domain/Interfaces/Repositories/IChatRepository.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Domain.Interfaces.Repositories;

/// <summary>Persistence contract for ChatMessage aggregate.</summary>
public interface IChatRepository
{
    Task<IReadOnlyList<ChatMessage>> GetByUserIdAsync(Guid userId, int limit, CancellationToken ct = default);
    Task AddAsync(ChatMessage message, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ChatMessage> messages, CancellationToken ct = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken ct = default);
}
