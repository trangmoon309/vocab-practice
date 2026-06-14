// VocaPlay.Infrastructure/Persistence/Repositories/WordSetRepository.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Persistence;

namespace VocaPlay.Infrastructure.Persistence.Repositories;

public class WordSetRepository : IWordSetRepository
{
    private readonly AppDbContext _db;

    public WordSetRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<WordSet>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.WordSets
            .Include(s => s.Words)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<WordSet?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.WordSets.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<WordSet?> GetByIdWithWordsAsync(Guid id, CancellationToken ct = default)
        => await _db.WordSets
            .Include(s => s.Words)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task AddAsync(WordSet wordSet, CancellationToken ct = default)
    {
        _db.WordSets.Add(wordSet);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WordSet wordSet, CancellationToken ct = default)
    {
        _db.WordSets.Update(wordSet);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(WordSet wordSet, CancellationToken ct = default)
    {
        _db.WordSets.Remove(wordSet);
        await _db.SaveChangesAsync(ct);
    }
}
