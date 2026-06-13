// VocaPlay.Application/Game/Queries/GetGamePairsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Game.Queries;

public class GetGamePairsQueryHandler
{
    private readonly IWordSetRepository _wordSets;

    public GetGamePairsQueryHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Returns shuffled word pairs for the matching game. Requires at least 4 pairs.</summary>
    public async Task<GamePairsDto> Handle(GetGamePairsQuery query, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdWithWordsAsync(query.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), query.WordSetId);

        if (set.UserId != query.UserId)
            throw new ForbiddenException();

        if (set.Words.Count < 4)
            throw new ValidationException("A word set needs at least 4 words to start a game.");

        var pairs = set.Words
            .OrderBy(_ => Guid.NewGuid())
            .Select(w => new GamePairItem(w.Id, w.English, w.Vietnamese))
            .ToList();

        return new GamePairsDto(set.Id, set.Title, pairs);
    }
}
