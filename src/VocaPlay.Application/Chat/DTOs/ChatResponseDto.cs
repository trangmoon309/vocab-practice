// VocaPlay.Application/Chat/DTOs/ChatResponseDto.cs
namespace VocaPlay.Application.Chat.DTOs;

public record ChatActionDto(string Type, Guid? WordSetId, int? WordsAdded);

public record ChatResponseDto(string Reply, ChatActionDto? Action);

public record ChatMessageDto(Guid Id, string Role, string Content, DateTime CreatedAt);
