// VocaPlay.Application/Words/Commands/BulkAddWordsCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.DTOs;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;

namespace VocaPlay.Application.Words.Commands;

public class BulkAddWordsCommandHandler
{
    private readonly IWordSetRepository _wordSets;
    private readonly IWordRepository _words;

    public BulkAddWordsCommandHandler(IWordSetRepository wordSets, IWordRepository words)
    {
        _wordSets = wordSets;
        _words = words;
    }

    /// <summary>
    /// Bulk-adds words to a set, skipping duplicates by English term (case-insensitive).
    /// Never throws on duplicate — returns skip reasons instead.
    /// </summary>
    public async Task<BulkAddResultDto> Handle(BulkAddWordsCommand command, CancellationToken ct = default)
    {
        var set = await _wordSets.GetByIdAsync(command.WordSetId, ct)
            ?? throw new NotFoundException(nameof(WordSet), command.WordSetId);

        if (set.UserId != command.UserId)
            throw new ForbiddenException();

        var existingEnglish = (await _words.GetEnglishWordsInSetAsync(command.WordSetId, ct))
            .Select(e => e.ToLowerInvariant())
            .ToHashSet();

        var toAdd = new List<Word>();
        var skippedReasons = new List<string>();

        foreach (var input in command.Words)
        {
            var key = input.English.Trim().ToLowerInvariant();
            if (existingEnglish.Contains(key))
            {
                skippedReasons.Add($"duplicate: {input.English.Trim()}");
                continue;
            }

            existingEnglish.Add(key);
            toAdd.Add(new Word
            {
                Id = Guid.NewGuid(),
                WordSetId = command.WordSetId,
                English = input.English.Trim(),
                Vietnamese = input.Vietnamese.Trim(),
                Pronunciation = input.Pronunciation?.Trim(),
                Level = input.Level,
                Type = input.Type,
                ExampleSentence = input.ExampleSentence?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (toAdd.Count > 0)
            await _words.AddRangeAsync(toAdd, ct);

        return new BulkAddResultDto(toAdd.Count, skippedReasons.Count, skippedReasons);
    }
}
