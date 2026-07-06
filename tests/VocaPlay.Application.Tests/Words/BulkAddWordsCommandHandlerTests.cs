using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Words;

public class BulkAddWordsCommandHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly BulkAddWordsCommandHandler _handler;

    public BulkAddWordsCommandHandlerTests()
    {
        _handler = new BulkAddWordsCommandHandler(_words.Object);
    }

    [Fact]
    public async Task Handle_AddsAllWords_WhenNoDuplicates()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetEnglishWordsForUserAsync(userId, default)).ReturnsAsync(new List<string>());

        var input = new List<WordInput>
        {
            new("apple", "qua tao", null, "A1", "Noun", null, null),
            new("book", "sach", null, "A1", "Noun", null, null),
        };

        var result = await _handler.Handle(new BulkAddWordsCommand(userId, input));

        Assert.Equal(2, result.Added);
        Assert.Equal(0, result.Skipped);
        _words.Verify(r => r.AddRangeAsync(It.Is<IEnumerable<Word>>(ws => ws.Count() == 2), default), Times.Once);
    }

    [Fact]
    public async Task Handle_SkipsDuplicates_CaseInsensitive_AgainstExistingWords()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetEnglishWordsForUserAsync(userId, default)).ReturnsAsync(new List<string> { "Apple" });

        var input = new List<WordInput> { new("apple", "qua tao", null, null, null, null, null) };

        var result = await _handler.Handle(new BulkAddWordsCommand(userId, input));

        Assert.Equal(0, result.Added);
        Assert.Equal(1, result.Skipped);
        Assert.Contains("duplicate: apple", result.SkippedReasons);
        _words.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Word>>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_SkipsDuplicates_WithinSameBatch()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetEnglishWordsForUserAsync(userId, default)).ReturnsAsync(new List<string>());

        var input = new List<WordInput>
        {
            new("apple", "qua tao 1", null, null, null, null, null),
            new("Apple", "qua tao 2", null, null, null, null, null),
        };

        var result = await _handler.Handle(new BulkAddWordsCommand(userId, input));

        Assert.Equal(1, result.Added);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task Handle_DoesNotCallAddRange_WhenAllSkipped()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetEnglishWordsForUserAsync(userId, default)).ReturnsAsync(new List<string> { "apple" });

        await _handler.Handle(new BulkAddWordsCommand(userId, new List<WordInput> { new("apple", "x", null, null, null, null, null) }));

        _words.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Word>>(), default), Times.Never);
    }
}
