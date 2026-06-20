// VocaPlay.Application/Game/Queries/GetGamePairsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Game.Queries;

public class GetGamePairsQueryHandler
{
    private readonly IWordRepository _words;

    public GetGamePairsQueryHandler(IWordRepository words) => _words = words;

    /// <summary>Returns shuffled word pairs for the matching game. Requires at least 4 words.</summary>
    public async Task<GamePairsDto> Handle(GetGamePairsQuery query, CancellationToken ct = default)
    {
        var words = await _words.GetByUserIdAsync(query.UserId, ct);

        if (words.Count < 4)
            throw new ValidationException("You need at least 4 words to start a game.");

        var pairs = words
            .OrderBy(_ => Guid.NewGuid())
            .Select(w => new GamePairItem(w.Id, w.English, w.Vietnamese))
            .ToList();

        return new GamePairsDto(pairs);
    }
}
