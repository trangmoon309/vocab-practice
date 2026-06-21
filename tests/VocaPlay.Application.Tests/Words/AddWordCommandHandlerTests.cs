using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;
using VocaPlay.Domain.Exceptions;
using Xunit;

namespace VocaPlay.Application.Tests.Words;

public class AddWordCommandHandlerTests
{
    private readonly Mock<IWordRepository> _words = new();
    private readonly AddWordCommandHandler _handler;

    public AddWordCommandHandlerTests()
    {
        _handler = new AddWordCommandHandler(_words.Object);
    }

    [Fact]
    public async Task Handle_AddsWord_ForCurrentUser()
    {
        var userId = Guid.NewGuid();
        Word? captured = null;
        _words.Setup(r => r.AddAsync(It.IsAny<Word>(), default))
            .Callback<Word, CancellationToken>((w, _) => captured = w)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new AddWordCommand(
            userId, "apple", "qua tao", "/ae.pel/", "A1", "Noun", "I eat an apple."));

        Assert.Equal("apple", result.English);
        Assert.Equal("qua tao", result.Vietnamese);
        Assert.Equal(userId, captured!.UserId);
    }

    [Fact]
    public async Task Handle_TrimsTextFields()
    {
        _words.Setup(r => r.AddAsync(It.IsAny<Word>(), default)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new AddWordCommand(
            Guid.NewGuid(), "  apple  ", "  qua tao  ", null, null, null, null));

        Assert.Equal("apple", result.English);
        Assert.Equal("qua tao", result.Vietnamese);
    }

    [Theory]
    [InlineData("Z9")]
    [InlineData("invalid")]
    public async Task Handle_Throws_WhenLevelInvalid(string level)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new AddWordCommand(Guid.NewGuid(), "apple", "qua tao", null, level, null, null)));

        _words.Verify(r => r.AddAsync(It.IsAny<Word>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_Throws_WhenTypeInvalid()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(new AddWordCommand(Guid.NewGuid(), "apple", "qua tao", null, null, "NotAType", null)));
    }
}
