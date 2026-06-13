// VocaPlay.Application/Chat/Queries/GetChatHistoryQueryHandler.cs
using VocaPlay.Application.Chat.DTOs;
using VocaPlay.Application.Common.Interfaces.Repositories;

namespace VocaPlay.Application.Chat.Queries;

public class GetChatHistoryQueryHandler
{
    private readonly IChatRepository _chat;

    public GetChatHistoryQueryHandler(IChatRepository chat) => _chat = chat;

    /// <summary>Returns the last N chat messages for the current user, oldest first.</summary>
    public async Task<IReadOnlyList<ChatMessageDto>> Handle(GetChatHistoryQuery query, CancellationToken ct = default)
    {
        var messages = await _chat.GetByUserIdAsync(query.UserId, query.Limit, ct);
        return messages.Select(m => new ChatMessageDto(m.Id, m.Role, m.Content, m.CreatedAt)).ToList();
    }
}
