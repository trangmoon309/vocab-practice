// VocaPlay.Api/Services/CurrentUserService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VocaPlay.Application.Common.Interfaces;

namespace VocaPlay.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("No authenticated user found.");
        }
    }
}
