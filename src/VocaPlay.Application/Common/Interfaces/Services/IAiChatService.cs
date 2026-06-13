// VocaPlay.Application/Common/Interfaces/Services/IAiChatService.cs
using VocaPlay.Domain.Entities;

namespace VocaPlay.Application.Common.Interfaces.Services;

/// <summary>Abstraction over the OpenAI chat completion service.</summary>
public interface IAiChatService
{
    /// <summary>
    /// Sends a user message and conversation history to the AI and returns the raw response text.
    /// </summary>
    Task<string> GetCompletionAsync(string userMessage, IEnumerable<ChatMessage> history, CancellationToken ct = default);
}
