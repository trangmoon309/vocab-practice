// VocaPlay.Application/Auth/Commands/LoginCommandHandler.cs
using VocaPlay.Application.Auth.DTOs;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Auth.Commands;

public class LoginCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;

    public LoginCommandHandler(IUserRepository users, IPasswordHasher hasher, IJwtTokenService jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    /// <summary>Authenticates a user and returns auth tokens.</summary>
    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(command.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !_hasher.Verify(command.Password, user.PasswordHash))
            throw new ValidationException("Invalid email or password.");

        return new AuthResponseDto(
            _jwt.GenerateAccessToken(user),
            _jwt.GenerateRefreshToken(user),
            new UserDto(user.Id, user.Email, user.DisplayName)
        );
    }
}
