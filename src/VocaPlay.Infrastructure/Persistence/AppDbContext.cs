// VocaPlay.Infrastructure/Persistence/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using VocaPlay.Domain.Entities;
using VocaPlay.Infrastructure.Configurations;

namespace VocaPlay.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WordConfiguration());
        modelBuilder.ApplyConfiguration(new GameSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());

        // Replace PostgreSQL NOW() with SQL Server GETUTCDATE() when using SQL Server provider
        if (Database.IsSqlServer())
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
                foreach (var prop in entity.GetProperties())
                    if (prop.GetDefaultValueSql() == "NOW()")
                        prop.SetDefaultValueSql("GETUTCDATE()");
        }

        base.OnModelCreating(modelBuilder);
    }
}
