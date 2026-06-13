// VocaPlay.Application/Chat/Commands/ClearChatHistoryCommandHandler.cs
using VocaPlay.Application.Common.Interfaces.Repositories;

namespace VocaPlay.Application.Chat.Commands;

public class ClearChatHistoryCommandHandler
{
    private readonly IChatRepository _chat;

    public ClearChatHistoryCommandHandler(IChatRepository chat) => _chat = chat;

    /// <summary>Deletes all chat messages for the current user.</summary>
    public Task Handle(ClearChatHistoryCommand command, CancellationToken ct = default)
        => _chat.DeleteByUserIdAsync(command.UserId, ct);
}
