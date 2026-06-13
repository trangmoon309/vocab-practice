// VocaPlay.Application/WordSets/Commands/CreateWordSetCommand.cs
namespace VocaPlay.Application.WordSets.Commands;

public record CreateWordSetCommand(Guid UserId, string Title, string? Description);
