using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.Queries;
using VocaPlay.Domain.Entities;
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

    private static List<Word> MakeWords(int count, Guid userId) =>
        Enumerable.Range(1, count)
            .Select(i => new Word { Id = Guid.NewGuid(), UserId = userId, English = $"word{i}", Vietnamese = $"tu{i}" })
            .ToList();

    [Fact]
    public async Task Handle_ReturnsAllPairs_WhenAtLeastFourWords()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(MakeWords(6, userId));

        var result = await _handler.Handle(new GetGamePairsQuery(userId));

        Assert.Equal(6, result.Pairs.Count);
    }

    [Fact]
    public async Task Handle_Throws_WhenFewerThanFourWords()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(MakeWords(3, userId));

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(new GetGamePairsQuery(userId)));
    }

    [Fact]
    public async Task Handle_Throws_WhenNoWords()
    {
        _words.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new List<Word>());

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(new GetGamePairsQuery(Guid.NewGuid())));
    }
}
