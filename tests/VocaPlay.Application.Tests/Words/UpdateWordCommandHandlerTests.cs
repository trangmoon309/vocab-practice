using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Words;

public class UpdateWordCommandHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly UpdateWordCommandHandler _handler;

    public UpdateWordCommandHandlerTests()
    {
        _handler = new UpdateWordCommandHandler(_words.Object);
    }

    [Fact]
    public async Task Handle_UpdatesWord_WhenOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var wordId = Guid.NewGuid();
        var word = new Word { Id = wordId, UserId = userId, English = "old", Vietnamese = "cu" };
        _words.Setup(r => r.GetByIdAsync(wordId, default)).ReturnsAsync(word);

        var result = await _handler.Handle(new UpdateWordCommand(
            wordId, userId, "new", "moi", null, "B1", "Verb", null, null));

        Assert.Equal("new", result.English);
        Assert.Equal("moi", result.Vietnamese);
        Assert.Equal("B1", result.Level);
        _words.Verify(r => r.UpdateAsync(word, default), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenWordNotFound()
    {
        _words.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Word?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new UpdateWordCommand(Guid.NewGuid(), Guid.NewGuid(), "x", "y", null, null, null, null, null)));
    }

    [Fact]
    public async Task Handle_Throws_WhenWordBelongsToAnotherUser()
    {
        var word = new Word { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _words.Setup(r => r.GetByIdAsync(word.Id, default)).ReturnsAsync(word);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(new UpdateWordCommand(word.Id, Guid.NewGuid(), "x", "y", null, null, null, null, null)));
    }
}
