// VocaPlay.Application/Chat/Commands/SendChatMessageCommand.cs
namespace VocaPlay.Application.Chat.Commands;

public record SendChatMessageCommand(Guid UserId, string Message, Guid? WordSetId);
