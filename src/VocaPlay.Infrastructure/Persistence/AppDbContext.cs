// VocaPlay.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Configurations;

namespace VocaPlay.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<WordSet> WordSets => Set<WordSet>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WordSetConfiguration());
        modelBuilder.ApplyConfiguration(new WordConfiguration());
        modelBuilder.ApplyConfiguration(new GameSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
