SKILL: OpenAI GPT-4o integration in .NET
- Use official OpenAI NuGet: OpenAI (v2+)
- Inject IOpenAIClient via DI — never instantiate directly in services
- Always wrap OpenAI calls in try/catch — return graceful fallback message on failure
- Sliding window: send last N messages from ChatMessage table as context
- Parse %%ACTION%%...%%END%% blocks AFTER stripping from user-visible reply
- Log OpenAI token usage to console in Development environment only
- Never log the full message content (PII risk)
