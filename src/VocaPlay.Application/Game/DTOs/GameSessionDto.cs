// VocaPlay.Application/Game/DTOs/GameSessionDto.cs
namespace VocaPlay.Application.Game.DTOs;

public record GameSessionDto(Guid Id, Guid WordSetId, int Score, int TotalPairs, DateTime CompletedAt);
