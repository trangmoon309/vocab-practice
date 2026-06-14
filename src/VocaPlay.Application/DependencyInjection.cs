// VocaPlay.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Chat.Commands;
using VocaPlay.Application.Chat.Queries;
using VocaPlay.Application.Game.Commands;
using VocaPlay.Application.Game.Queries;
using VocaPlay.Application.WordSets.Commands;
using VocaPlay.Application.WordSets.Queries;
using VocaPlay.Application.Words.Commands;

namespace VocaPlay.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<RefreshTokenCommandHandler>();

        // WordSets
        services.AddScoped<GetWordSetsQueryHandler>();
        services.AddScoped<GetWordSetByIdQueryHandler>();
        services.AddScoped<CreateWordSetCommandHandler>();
        services.AddScoped<UpdateWordSetCommandHandler>();
        services.AddScoped<DeleteWordSetCommandHandler>();

        // Words
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
