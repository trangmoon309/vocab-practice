// VocaPlay.Domain/Entities/User.cs
namespace VocaPlay.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Word> Words { get; set; } = new List<Word>();
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
