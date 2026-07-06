// VocaPlay.Application/Words/Queries/GetWordsQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.DTOs;

namespace VocaPlay.Application.Words.Queries;

public class GetWordsQueryHandler
{
    private readonly IWordRepository _words;

    public GetWordsQueryHandler(IWordRepository words) => _words = words;

    /// <summary>Returns all words belonging to the current user.</summary>
    public async Task<IReadOnlyList<WordDto>> Handle(GetWordsQuery query, CancellationToken ct = default)
    {
        var words = await _words.GetByUserIdAsync(query.UserId, ct);
        return words.Select(w => new WordDto(w.Id, w.English, w.Vietnamese, w.Pronunciation, w.Level, w.Type, w.ExampleSentence, w.EnglishDefinition)).ToList();
    }
}
