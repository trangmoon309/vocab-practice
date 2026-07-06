// VocaPlay.Domain/Entities/Word.cs
namespace VocaPlay.Domain.Entities;

public class Word
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string English { get; set; } = string.Empty;
    public string Vietnamese { get; set; } = string.Empty;
    public string? Pronunciation { get; set; }
    public string? Level { get; set; }
    public string? Type { get; set; }
    public string? ExampleSentence { get; set; }
    public string? EnglishDefinition { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
