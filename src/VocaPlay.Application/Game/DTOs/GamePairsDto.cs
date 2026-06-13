// VocaPlay.Application/Game/DTOs/GamePairsDto.cs
namespace VocaPlay.Application.Game.DTOs;

public record GamePairItem(Guid Id, string English, string Vietnamese);

public record GamePairsDto(Guid WordSetId, string WordSetTitle, IReadOnlyList<GamePairItem> Pairs);
