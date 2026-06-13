// VocaPlay.Application/Auth/Commands/RegisterCommandHandler.cs
using VocaPlay.Application.Auth.DTOs;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Auth.Commands;

public class RegisterCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public RegisterCommandHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    /// <summary>Registers a new user and returns auth tokens.</summary>
    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken ct = default)
    {
        var existing = await _users.GetByEmailAsync(command.Email, ct);
        if (existing is not null)
            throw new ValidationException("Email is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email.Trim().ToLowerInvariant(),
            DisplayName = command.DisplayName.Trim(),
            PasswordHash = _hasher.Hash(command.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);

        return new AuthResponseDto(
            _jwt.GenerateAccessToken(user),
            _jwt.GenerateRefreshToken(user),
            new UserDto(user.Id, user.Email, user.DisplayName)
        );
    }
}
