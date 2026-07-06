// VocaPlay.Application/Game/Queries/GetGamePairsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Enums;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Game.Queries;

public class GetGamePairsQueryHandler
{
    private readonly IWordRepository _words;

    public GetGamePairsQueryHandler(IWordRepository words) => _words = words;

    /// <summary>
    /// Returns shuffled word pairs for the matching game. Translation mode uses the Vietnamese
    /// meaning; Definition mode uses the English definition and excludes words missing one.
    /// Requires at least 4 eligible words.
    /// </summary>
    public async Task<GamePairsDto> Handle(GetGamePairsQuery query, CancellationToken ct = default)
    {
        var words = await _words.GetByUserIdAsync(query.UserId, ct);

        var eligible = query.Mode == GameMode.Definition
            ? words.Where(w => !string.IsNullOrWhiteSpace(w.EnglishDefinition)).ToList()
            : words;

        if (eligible.Count < 4)
        {
            var message = query.Mode == GameMode.Definition
                ? "You need at least 4 words with an English definition to play Definition Match."
                : "You need at least 4 words to start a game.";
            throw new ValidationException(message);
        }

        var pairs = eligible
            .OrderBy(_ => Guid.NewGuid())
            .Select(w => new GamePairItem(w.Id, w.English, MatchFor(w, query.Mode)))
            .ToList();

        return new GamePairsDto(query.Mode, pairs);
    }

    private static string MatchFor(Word word, GameMode mode) =>
        mode == GameMode.Definition ? word.EnglishDefinition! : word.Vietnamese;
}
