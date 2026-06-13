// VocaPlay.Application/Common/Interfaces/IJwtTokenService.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Common.Interfaces;

/// <summary>Generates and validates JWT access and refresh tokens.</summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);

    /// <summary>Validates a refresh token and returns the embedded UserId, or null if invalid.</summary>
    Guid? ValidateRefreshToken(string refreshToken);
}
