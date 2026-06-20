// VocaPlay.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Application.Chat.Queries;
using VocaPlay.Application.Game.Commands;
using VocaPlay.Application.Game.Queries;
using VocaPlay.Application.Words.Commands;
using VocaPlay.Application.Words.Queries;

namespace VocaPlay.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();

        // Words
        services.AddScoped<GetWordsQueryHandler>();
        services.AddScoped<AddWordCommandHandler>();
        services.AddScoped<UpdateWordCommandHandler>();
        services.AddScoped<DeleteWordCommandHandler>();
        services.AddScoped<BulkAddWordsCommandHandler>();

        // Game
        services.AddScoped<GetGamePairsQueryHandler>();
        services.AddScoped<SaveGameSessionCommandHandler>();
        services.AddScoped<GetGameSessionsQueryHandler>();

        // Chat
        services.AddScoped<SendChatMessageCommandHandler>();
        services.AddScoped<ClearChatHistoryCommandHandler>();
        services.AddScoped<GetChatHistoryQueryHandler>();

        return services;
    }
}
