// VocaPlay.Application/Chat/Queries/GetChatHistoryQuery.cs
namespace VocaPlay.Application.Chat.Queries;

public record GetChatHistoryQuery(Guid UserId, int Limit = 50);
