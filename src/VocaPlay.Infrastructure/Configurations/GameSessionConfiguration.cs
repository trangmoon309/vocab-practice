// VocaPlay.Infrastructure/Configurations/GameSessionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Infrastructure.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Score).IsRequired();
        builder.Property(e => e.TotalPairs).IsRequired();
        builder.Property(e => e.CompletedAt).HasDefaultValueSql("NOW()");
    }
}
