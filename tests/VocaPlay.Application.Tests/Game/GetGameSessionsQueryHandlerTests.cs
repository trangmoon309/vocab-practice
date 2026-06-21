using Moq;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Game.Queries;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Game;

public class GetGameSessionsQueryHandlerTests
{
    private readonly Mock<IGameSessionRepository> _sessions = new();
    private readonly GetGameSessionsQueryHandler _handler;

    public GetGameSessionsQueryHandlerTests()
    {
        _handler = new GetGameSessionsQueryHandler(_sessions.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSessions_ForGivenUser()
    {
        var userId = Guid.NewGuid();
        _sessions.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(new List<GameSession>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Score = 5, TotalPairs = 6, CompletedAt = DateTime.UtcNow },
        });

        var result = await _handler.Handle(new GetGameSessionsQuery(userId));

        Assert.Single(result);
        Assert.Equal(5, result[0].Score);
    }
}
