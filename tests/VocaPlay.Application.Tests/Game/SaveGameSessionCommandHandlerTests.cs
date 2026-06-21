using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.Commands;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Game;

public class SaveGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _sessions = new();
    private readonly SaveGameSessionCommandHandler _handler;

    public SaveGameSessionCommandHandlerTests()
    {
        _handler = new SaveGameSessionCommandHandler(_sessions.Object);
    }

    [Fact]
    public async Task Handle_PersistsSession_WithGivenScore()
    {
        var userId = Guid.NewGuid();
        GameSession? captured = null;
        _sessions.Setup(r => r.AddAsync(It.IsAny<GameSession>(), default))
            .Callback<GameSession, CancellationToken>((s, _) => captured = s)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new SaveGameSessionCommand(userId, 8, 10));

        Assert.Equal(8, result.Score);
        Assert.Equal(10, result.TotalPairs);
        Assert.Equal(userId, captured!.UserId);
        Assert.Equal(8, captured.Score);
    }
}
