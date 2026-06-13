// VocaPlay.Application/WordSets/Queries/GetWordSetsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.WordSets.DTOs;

namespace VocaPlay.Application.WordSets.Queries;

public class GetWordSetsQueryHandler
{
    private readonly IWordSetRepository _wordSets;

    public GetWordSetsQueryHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Returns all word sets belonging to the current user.</summary>
    public async Task<IReadOnlyList<WordSetDto>> Handle(GetWordSetsQuery query, CancellationToken ct = default)
    {
        var sets = await _wordSets.GetByUserIdAsync(query.UserId, ct);
        return sets.Select(s => new WordSetDto(s.Id, s.Title, s.Description, s.Words.Count, s.CreatedAt)).ToList();
    }
}
