using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.Queries;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Words;

public class GetWordsQueryHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly GetWordsQueryHandler _handler;

    public GetWordsQueryHandlerTests()
    {
        _handler = new GetWordsQueryHandler(_words.Object);
    }

    [Fact]
    public async Task Handle_ReturnsWords_ForGivenUser()
    {
        var userId = Guid.NewGuid();
        _words.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(new List<Word>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, English = "apple", Vietnamese = "qua tao" },
            new() { Id = Guid.NewGuid(), UserId = userId, English = "book", Vietnamese = "sach" },
        });

        var result = await _handler.Handle(new GetWordsQuery(userId));

        Assert.Equal(2, result.Count);
        Assert.Contains(result, w => w.English == "apple");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenUserHasNoWords()
    {
        _words.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new List<Word>());

        var result = await _handler.Handle(new GetWordsQuery(Guid.NewGuid()));

        Assert.Empty(result);
    }
}
