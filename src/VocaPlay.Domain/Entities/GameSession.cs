// VocaPlay.Domain/Entities/GameSession.cs
namespace VocaPlay.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public int TotalPairs { get; set; }
    public DateTime CompletedAt { get; set; }

    public User User { get; set; } = null!;
}
