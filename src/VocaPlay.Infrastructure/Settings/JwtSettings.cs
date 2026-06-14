// VocaPlay.Infrastructure/Settings/JwtSettings.cs
namespace VocaPlay.Infrastructure.Settings;

public class JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; init; } = 60;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
