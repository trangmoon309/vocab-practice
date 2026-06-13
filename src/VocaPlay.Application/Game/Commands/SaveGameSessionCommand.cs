// VocaPlay.Application/Game/Commands/SaveGameSessionCommand.cs
namespace VocaPlay.Application.Game.Commands;

public record SaveGameSessionCommand(Guid UserId, Guid WordSetId, int Score, int TotalPairs);
