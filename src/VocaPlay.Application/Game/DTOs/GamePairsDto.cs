// VocaPlay.Application/Game/DTOs/GamePairsDto.cs
using VocaPlay.Domain.Enums;

namespace VocaPlay.Application.Game.DTOs;

/// <summary>One matching pair. <see cref="English"/> is always the English word;
/// <see cref="Match"/> is the Vietnamese meaning (Translation mode) or the English
/// definition (Definition mode), depending on <see cref="GamePairsDto.Mode"/>.</summary>
public record GamePairItem(Guid Id, string English, string Match);

public record GamePairsDto(GameMode Mode, IReadOnlyList<GamePairItem> Pairs);
