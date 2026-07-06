// VocaPlay.Application/Chat/Commands/SendChatMessageCommandHandler.cs
using System.Text.Json;
using VocaPlay.Application.Chat.DTOs;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Common.Interfaces.Services;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Chat.Commands;

public class SendChatMessageCommandHandler
{
    private readonly IChatRepository _chat;
    private readonly IAiChatService _ai;
    private readonly BulkAddWordsCommandHandler _bulkAdd;
    private readonly int _maxHistory;

    public SendChatMessageCommandHandler(
        IChatRepository chat,
        IAiChatService ai,
        BulkAddWordsCommandHandler bulkAdd,
        int maxHistory = 10)
    {
        _chat = chat;
        _ai = ai;
        _bulkAdd = bulkAdd;
        _maxHistory = maxHistory;
    }

    /// <summary>
    /// Sends user message to GPT-4o, parses optional %%ACTION%%...%%END%% block,
    /// triggers bulk-add if present, persists both messages, returns clean reply + action.
    /// </summary>
    public async Task<ChatResponseDto> Handle(SendChatMessageCommand command, CancellationToken ct = default)
    {
        var history = await _chat.GetByUserIdAsync(command.UserId, _maxHistory, ct);

        var rawReply = await _ai.GetCompletionAsync(command.Message, history, ct);

        var (cleanReply, action) = await ParseAndExecuteAction(rawReply, command, ct);

        await _chat.AddRangeAsync(new[]
        {
            new ChatMessage { Id = Guid.NewGuid(), UserId = command.UserId, Role = "user",      Content = command.Message, CreatedAt = DateTime.UtcNow },
            new ChatMessage { Id = Guid.NewGuid(), UserId = command.UserId, Role = "assistant", Content = cleanReply,      CreatedAt = DateTime.UtcNow }
        }, ct);

        return new ChatResponseDto(cleanReply, action);
    }

    private async Task<(string reply, ChatActionDto? action)> ParseAndExecuteAction(
        string rawReply, SendChatMessageCommand command, CancellationToken ct)
    {
        const string start = "%%ACTION%%";
        const string end = "%%END%%";

        var startIdx = rawReply.IndexOf(start, StringComparison.Ordinal);
        var endIdx = rawReply.IndexOf(end, StringComparison.Ordinal);

        if (startIdx < 0 || endIdx < 0)
            return (rawReply, null);

        var json = rawReply[(startIdx + start.Length)..endIdx].Trim();
        var cleanReply = (rawReply[..startIdx] + rawReply[(endIdx + end.Length)..]).Trim();

        try
        {
            var payload = JsonSerializer.Deserialize<BulkActionPayload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (payload?.Type != "BULK_ADD_WORDS" || payload.Words is null)
                return (cleanReply, null);

            var inputs = payload.Words.Select(w => new WordInput(
                w.English, w.Vietnamese, w.Pronunciation, w.Level, w.Type, w.ExampleSentence, w.EnglishDefinition)).ToList();

            var result = await _bulkAdd.Handle(
                new BulkAddWordsCommand(command.UserId, inputs), ct);

            return (cleanReply, new ChatActionDto("BULK_ADD_WORDS", result.Added));
        }
        catch
        {
            return (cleanReply, null);
        }
    }

    private record BulkActionPayload(string Type, List<WordPayload>? Words);
    private record WordPayload(string English, string Vietnamese, string? Pronunciation, string? Level, string? Type, string? ExampleSentence, string? EnglishDefinition);
}
