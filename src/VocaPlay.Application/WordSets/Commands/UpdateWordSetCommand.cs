// VocaPlay.Application/WordSets/Commands/UpdateWordSetCommand.cs
namespace VocaPlay.Application.WordSets.Commands;

public record UpdateWordSetCommand(Guid WordSetId, Guid UserId, string Title, string? Description);
