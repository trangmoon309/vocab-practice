// VocaPlay.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VocaPlay.Application.Common.Interfaces;
using VocaPlay.Application.Common.Interfaces.Repositories;
using VocaPlay.Application.Common.Interfaces.Services;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Infrastructure.Auth;
using VocaPlay.Infrastructure.ExternalServices;
using VocaPlay.Infrastructure.Persistence;
using VocaPlay.Infrastructure.Persistence.Repositories;
using VocaPlay.Infrastructure.Settings;

namespace VocaPlay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Settings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAI"));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWordSetRepository, WordSetRepository>();
        services.AddScoped<IWordRepository, WordRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();

        // Auth services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // External services
        services.AddScoped<IAiChatService, OpenAiChatService>();

        // Re-register SendChatMessageCommandHandler with configured MaxHistoryMessages
        services.AddScoped<SendChatMessageCommandHandler>(sp =>
        {
            var maxHistory = configuration.GetValue<int>("OpenAI:MaxHistoryMessages", 10);
            return new SendChatMessageCommandHandler(
                sp.GetRequiredService<IChatRepository>(),
                sp.GetRequiredService<IAiChatService>(),
                sp.GetRequiredService<VocaPlay.Application.Words.Commands.BulkAddWordsCommandHandler>(),
                maxHistory);
        });

        return services;
    }
}
