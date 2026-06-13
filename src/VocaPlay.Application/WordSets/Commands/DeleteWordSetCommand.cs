// VocaPlay.Application/WordSets/Commands/DeleteWordSetCommand.cs
namespace VocaPlay.Application.WordSets.Commands;

public record DeleteWordSetCommand(Guid WordSetId, Guid UserId);
