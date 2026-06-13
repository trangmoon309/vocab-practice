// VocaPlay.Application/Auth/DTOs/AuthRequestDto.cs
namespace VocaPlay.Application.Auth.DTOs;

public record RegisterRequestDto(string Email, string DisplayName, string Password);

public record LoginRequestDto(string Email, string Password);

public record RefreshTokenRequestDto(string RefreshToken);
