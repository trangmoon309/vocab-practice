// VocaPlay.Application/Auth/DTOs/AuthResponseDto.cs
namespace VocaPlay.Application.Auth.DTOs;

public record AuthResponseDto(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string Email, string DisplayName);

public record TokenResponseDto(string AccessToken, string RefreshToken);
