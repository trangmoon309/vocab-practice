// VocaPlay.Application/Common/Interfaces/Repositories/IUserRepository.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Common.Interfaces.Repositories;

/// <summary>Persistence contract for User aggregate.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
