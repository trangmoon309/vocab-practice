// VocaPlay.Infrastructure/Persistence/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VocaPlay.Infrastructure.Persistence;

/// <summary>Used by dotnet-ef CLI to create migrations without a running app.</summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=vocaplay;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
