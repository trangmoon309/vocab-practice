using Moq;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Application.Common.Interfaces.Repositories;
using Xunit;

namespace VocaPlay.Application.Tests.Chat;

public class ClearChatHistoryCommandHandlerTests
{
    private readonly Mock<IChatRepository> _chat = new();
    private readonly ClearChatHistoryCommandHandler _handler;

    public ClearChatHistoryCommandHandlerTests()
    {
        _handler = new ClearChatHistoryCommandHandler(_chat.Object);
    }

    [Fact]
    public async Task Handle_DeletesAllMessages_ForGivenUser()
    {
        var userId = Guid.NewGuid();

        await _handler.Handle(new ClearChatHistoryCommand(userId));

        _chat.Verify(r => r.DeleteByUserIdAsync(userId, default), Times.Once);
    }
}
