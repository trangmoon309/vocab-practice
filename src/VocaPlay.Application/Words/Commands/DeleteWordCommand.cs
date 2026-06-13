// VocaPlay.Application/Words/Commands/DeleteWordCommand.cs
namespace VocaPlay.Application.Words.Commands;

public record DeleteWordCommand(Guid WordId, Guid WordSetId, Guid UserId);
