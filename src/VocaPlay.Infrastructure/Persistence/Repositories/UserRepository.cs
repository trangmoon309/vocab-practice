// VocaPlay.Infrastructure/Persistence/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Persistence;

namespace VocaPlay.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Users.FindAsync(new object[] { id }, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
