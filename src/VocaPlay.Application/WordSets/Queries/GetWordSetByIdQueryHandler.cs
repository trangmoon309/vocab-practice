// VocaPlay.Application/WordSets/Queries/GetWordSetByIdQueryHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.DTOs;
using VocaPlay.Application.WordSets.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.WordSets.Queries;

public class GetWordSetByIdQueryHandler
{
    private readonly IWordSetRepository _wordSets;

    public GetWordSetByIdQueryHandler(IWordSetRepository wordSets) => _wordSets = wordSets;

    /// <summary>Returns a word set with its words. Enforces ownership.</summary>
    public async Task<WordSetDetailDto> Handle(GetWordSetByIdQuery query, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdWithWordsAsync(query.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), query.WordSetId);

        if (set.UserId != query.UserId)
            throw new ForbiddenException();

        var words = set.Words.Select(w => new WordDto(
            w.Id, w.English, w.Vietnamese, w.Pronunciation, w.Level, w.Type, w.ExampleSentence)).ToList();

        return new WordSetDetailDto(set.Id, set.Title, set.Description, words.Count, words, set.CreatedAt);
    }
}
