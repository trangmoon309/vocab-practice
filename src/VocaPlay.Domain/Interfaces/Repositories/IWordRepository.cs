// VocaPlay.Domain/Interfaces/Repositories/IWordRepository.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Domain.Interfaces.Repositories;

/// <summary>Persistence contract for Word aggregate.</summary>
public interface IWordRepository
{
    Task<Word?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Word>> GetByWordSetIdAsync(Guid wordSetId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetEnglishWordsInSetAsync(Guid wordSetId, CancellationToken ct = default);
    Task AddAsync(Word word, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Word> words, CancellationToken ct = default);
    Task UpdateAsync(Word word, CancellationToken ct = default);
    Task DeleteAsync(Word word, CancellationToken ct = default);
}
