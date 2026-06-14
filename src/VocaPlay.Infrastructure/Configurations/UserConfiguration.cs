// VocaPlay.Infrastructure/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VocaPlay.Domain.Entities;

namespace VocaPlay.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PasswordHash).IsRequired();

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.Email).IsUnique();

        builder.HasMany(e => e.WordSets)
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.GameSessions)
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ChatMessages)
               .WithOne(e => e.User)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
