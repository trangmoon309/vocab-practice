// VocaPlay.Domain/Entities/ChatMessage.cs
namespace VocaPlay.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
