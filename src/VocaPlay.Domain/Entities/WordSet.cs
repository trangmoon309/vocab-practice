// VocaPlay.Domain/Entities/WordSet.cs
namespace VocaPlay.Domain.Entities;

public class WordSet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Word> Words { get; set; } = new List<Word>();
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
}
