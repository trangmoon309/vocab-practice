// VocaPlay.Infrastructure/Configurations/WordSetConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Infrastructure.Configurations;

public class WordSetConfiguration : IEntityTypeConfiguration<WordSet>
{
    public void Configure(EntityTypeBuilder<WordSet> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasMany(e => e.Words)
               .WithOne(e => e.WordSet)
               .HasForeignKey(e => e.WordSetId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.GameSessions)
               .WithOne(e => e.WordSet)
               .HasForeignKey(e => e.WordSetId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
