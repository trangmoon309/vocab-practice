// VocaPlay.Application/Common/Interfaces/Repositories/IWordSetRepository.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Common.Interfaces.Repositories;

/// <summary>Persistence contract for WordSet aggregate.</summary>
public interface IWordSetRepository
{
    Task<IReadOnlyList<WordSet>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<WordSet?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WordSet?> GetByIdWithWordsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(WordSet wordSet, CancellationToken ct = default);
    Task UpdateAsync(WordSet wordSet, CancellationToken ct = default);
    Task DeleteAsync(WordSet wordSet, CancellationToken ct = default);
}
