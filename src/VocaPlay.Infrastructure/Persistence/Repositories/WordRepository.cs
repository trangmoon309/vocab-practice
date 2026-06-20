// VocaPlay.Infrastructure/Persistence/Repositories/WordRepository.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Persistence;

namespace VocaPlay.Infrastructure.Persistence.Repositories;

public class WordRepository : IWordRepository
{
    private readonly AppDbContext _db;

    public WordRepository(AppDbContext db) => _db = db;

    public async Task<Word?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Words.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Word>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Words
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetEnglishWordsForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Words
            .Where(w => w.UserId == userId)
            .Select(w => w.English)
            .ToListAsync(ct);

    public async Task AddAsync(Word word, CancellationToken ct = default)
    {
        _db.Words.Add(word);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<Word> words, CancellationToken ct = default)
    {
        _db.Words.AddRange(words);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Word word, CancellationToken ct = default)
    {
        _db.Words.Update(word);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Word word, CancellationToken ct = default)
    {
        _db.Words.Remove(word);
        await _db.SaveChangesAsync(ct);
    }
}
