// VocaPlay.Application/Auth/Commands/RegisterCommand.cs
namespace VocaPlay.Application.Auth.Commands;

public record RegisterCommand(string Email, string DisplayName, string Password);
