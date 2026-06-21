using Moq;
using VocaPlay.Application.Chat.Queries;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Domain.Entities;
using Xunit;

namespace VocaPlay.Application.Tests.Chat;

public class GetChatHistoryQueryHandlerTests
{
    private readonly Mock<IChatRepository> _chat = new();
    private readonly GetChatHistoryQueryHandler _handler;

    public GetChatHistoryQueryHandlerTests()
    {
        _handler = new GetChatHistoryQueryHandler(_chat.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedMessages_ForGivenUserAndLimit()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 50, default)).ReturnsAsync(new List<ChatMessage>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Role = "user", Content = "hi", CreatedAt = DateTime.UtcNow },
        });

        var result = await _handler.Handle(new GetChatHistoryQuery(userId));

        Assert.Single(result);
        Assert.Equal("hi", result[0].Content);
        Assert.Equal("user", result[0].Role);
    }

    [Fact]
    public async Task Handle_PassesCustomLimit_ToRepository()
    {
        var userId = Guid.NewGuid();
        _chat.Setup(r => r.GetByUserIdAsync(userId, 5, default)).ReturnsAsync(new List<ChatMessage>());

        await _handler.Handle(new GetChatHistoryQuery(userId, 5));

        _chat.Verify(r => r.GetByUserIdAsync(userId, 5, default), Times.Once);
    }
}
