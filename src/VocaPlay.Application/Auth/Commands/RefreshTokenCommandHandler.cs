// VocaPlay.Application/Auth/Commands/RefreshTokenCommandHandler.cs
using VocaPlay.Application.Auth.DTOs;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Auth.Commands;

public class RefreshTokenCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwt;

    public RefreshTokenCommandHandler(IUserRepository users, IJwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    /// <summary>Validates a refresh token and issues a new token pair.</summary>
    public async Task<TokenResponseDto> Handle(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var userId = _jwt.ValidateRefreshToken(command.RefreshToken)
            ?? throw new ValidationException("Invalid or expired refresh token.");

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new ValidationException("Invalid or expired refresh token.");

        return new TokenResponseDto(
            _jwt.GenerateAccessToken(user),
            _jwt.GenerateRefreshToken(user)
        );
    }
}
