// VocaPlay.Infrastructure/Settings/OpenAiSettings.cs
namespace VocaPlay.Infrastructure.Settings;

public class OpenAiSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4o";
    public int MaxHistoryMessages { get; init; } = 10;
}
