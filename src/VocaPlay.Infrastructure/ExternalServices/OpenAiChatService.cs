// VocaPlay.Infrastructure/ExternalServices/OpenAiChatService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using VocaPlay.Application.Common.Interfaces.Services;
using VocaPlay.Infrastructure.Settings;
using ChatMessage = VocaPlay.Domain.Entities.ChatMessage;
using OpenAiChatMessage = OpenAI.Chat.ChatMessage;

namespace VocaPlay.Infrastructure.ExternalServices;

public class OpenAiChatService : IAiChatService
{
    private const string SystemPrompt = """
        You are VocaPlay Assistant, an AI tutor helping Vietnamese learners build English vocabulary.
        You respond in a mix of English and Vietnamese — use Vietnamese to explain meanings and grammar,
        use English for the vocab terms themselves.

        You have three jobs:
        1. BULK ADD WORDS: When the user gives you a list of words to add to a word set, parse them,
           auto-fill missing Vietnamese translations, pronunciation (IPA), CEFR level (A1–C2), and
           word type (Noun/Verb/etc.) using your knowledge, and respond with a JSON action block so
           the backend can call the bulk-add API.
        2. EXPLAIN: Explain English words in simple Vietnamese, give example sentences, pronunciation tips.
        3. QUIZ: Run a short text quiz using the user's own vocabulary. Ask one question at a time,
           wait for the answer, give feedback, then ask the next.

        When performing a BULK_ADD action, always end your reply with this exact JSON block on its own line:
        %%ACTION%%{"type":"BULK_ADD_WORDS","words":[{"english":"...","vietnamese":"...","pronunciation":"...","level":"B1","type":"Noun","exampleSentence":"..."}]}%%END%%

        Keep replies friendly, concise, and encouraging. Use 🌟 sparingly for correct answers.
        """;

    private readonly ChatClient _client;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiChatService> _logger;

    public OpenAiChatService(IOptions<OpenAiSettings> settings, ILogger<OpenAiChatService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new ChatClient(_settings.Model, _settings.ApiKey);
    }

    public async Task<string> GetCompletionAsync(string userMessage, IEnumerable<ChatMessage> history, CancellationToken ct = default)
    {
        var messages = new List<OpenAiChatMessage>
        {
            new SystemChatMessage(SystemPrompt)
        };

        foreach (var h in history)
        {
            messages.Add(h.Role == "user"
                ? new UserChatMessage(h.Content)
                : new AssistantChatMessage(h.Content));
        }

        messages.Add(new UserChatMessage(userMessage));

        try
        {
            var completion = await _client.CompleteChatAsync(messages, cancellationToken: ct);
            var result = completion.Value;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                _logger.LogInformation("OpenAI usage — input: {Input} tokens, output: {Output} tokens",
                    result.Usage.InputTokenCount, result.Usage.OutputTokenCount);

            return result.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI call failed");
            return "Sorry, I'm having trouble right now. Please try again.";
        }
    }
}
