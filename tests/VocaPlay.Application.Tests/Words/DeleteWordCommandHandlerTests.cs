using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Words;

public class DeleteWordCommandHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly DeleteWordCommandHandler _handler;

    public DeleteWordCommandHandlerTests()
    {
        _handler = new DeleteWordCommandHandler(_words.Object);
    }

    [Fact]
    public async Task Handle_DeletesWord_WhenOwnedByUser()
    {
        var userId = Guid.NewGuid();
        var word = new Word { Id = Guid.NewGuid(), UserId = userId };
        _words.Setup(r => r.GetByIdAsync(word.Id, default)).ReturnsAsync(word);

        await _handler.Handle(new DeleteWordCommand(word.Id, userId));

        _words.Verify(r => r.DeleteAsync(word, default), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_WhenWordNotFound()
    {
        _words.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Word?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new DeleteWordCommand(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task Handle_Throws_WhenWordBelongsToAnotherUser()
    {
        var word = new Word { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _words.Setup(r => r.GetByIdAsync(word.Id, default)).ReturnsAsync(word);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(new DeleteWordCommand(word.Id, Guid.NewGuid())));

        _words.Verify(r => r.DeleteAsync(It.IsAny<Word>(), default), Times.Never);
    }
}
