// VocaPlay.Infrastructure/Configurations/WordConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Infrastructure.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.English).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Vietnamese).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Pronunciation).HasMaxLength(200);
        builder.Property(e => e.Level).HasMaxLength(10);
        builder.Property(e => e.Type).HasMaxLength(50);
        builder.Property(e => e.ExampleSentence).HasMaxLength(500);
        builder.Property(e => e.EnglishDefinition).HasMaxLength(500);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.UserId);
    }
}
