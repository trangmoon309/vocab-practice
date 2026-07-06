// VocaPlay.Application/Game/Queries/GetGamePairsQuery.cs
using VocaPlay.Domain.Enums;

namespace VocaPlay.Application.Game.Queries;

public record GetGamePairsQuery(Guid UserId, GameMode Mode);
