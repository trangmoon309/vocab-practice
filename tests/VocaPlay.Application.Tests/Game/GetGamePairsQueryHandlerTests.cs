using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.Queries;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Enums;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Game;

public class GetGamePairsQueryHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly GetGamePairsQueryHandler _handler;

    public GetGamePairsQueryHandlerTests()
    {
        _handler = new GetGamePairsQueryHandler(_words.Object);
    }

    private static List<Word> MakeWords(int count, Guid userId, bool withDefinitions = false) =>
        Enumerable.Range(1, count)
            .Select(i => new Word
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                English = $"word{i}",
                Vietnamese = $"tu{i}",
                EnglishDefinition = withDefinitions ? $"definition of word{i}" : null
            })
            .ToList();

    [Fact]
    public async Task Handle_ReturnsAllPairs_WhenAtLeastFourWords_TranslationMode()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(MakeWords(6, userId));

        var result = await _handler.Handle(new GetGamePairsQuery(userId, GameMode.Translation));

        Assert.Equal(6, result.Pairs.Count);
        Assert.Equal(GameMode.Translation, result.Mode);
        Assert.All(result.Pairs, p => Assert.StartsWith("tu", p.Match));
    }

    [Fact]
    public async Task Handle_Throws_WhenFewerThanFourWords_TranslationMode()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(MakeWords(3, userId));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new GetGamePairsQuery(userId, GameMode.Translation)));
    }

    [Fact]
    public async Task Handle_Throws_WhenNoWords()
    {
        _words.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new List<Word>());

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new GetGamePairsQuery(Guid.NewGuid(), GameMode.Translation)));
    }

    [Fact]
    public async Task Handle_ReturnsDefinitionPairs_WhenAtLeastFourWordsHaveDefinitions()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(MakeWords(5, userId, withDefinitions: true));

        var result = await _handler.Handle(new GetGamePairsQuery(userId, GameMode.Definition));

        Assert.Equal(5, result.Pairs.Count);
        Assert.Equal(GameMode.Definition, result.Mode);
        Assert.All(result.Pairs, p => Assert.StartsWith("definition of", p.Match));
    }

    [Fact]
    public async Task Handle_Throws_DefinitionMode_WhenFewerThanFourWordsHaveDefinitions()
    {
        var userId = Guid.NewGuid();
        var words = MakeWords(3, userId, withDefinitions: true)
            .Concat(MakeWords(5, userId, withDefinitions: false))
            .ToList();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(words);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new GetGamePairsQuery(userId, GameMode.Definition)));
    }

    [Fact]
    public async Task Handle_DefinitionMode_ExcludesWordsWithoutDefinition()
    {
        var userId = Guid.NewGuid();
        var withDefs = MakeWords(4, userId, withDefinitions: true);
        var withoutDefs = MakeWords(4, userId, withDefinitions: false);
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(withDefs.Concat(withoutDefs).ToList());

        var result = await _handler.Handle(new GetGamePairsQuery(userId, GameMode.Definition));

        Assert.Equal(4, result.Pairs.Count);
    }
}
