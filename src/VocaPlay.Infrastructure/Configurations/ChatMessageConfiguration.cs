// VocaPlay.Infrastructure/Configurations/ChatMessageConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Infrastructure.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Role).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(4000);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
    }
}
