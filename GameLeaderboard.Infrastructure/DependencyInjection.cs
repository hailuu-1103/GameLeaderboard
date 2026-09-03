namespace GameLeaderboard.Infrastructure;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Infrastructure.Persistence.InMemory;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<InMemoryLeaderboardStore>();

        services.AddSingleton<
            ILeaderboardRepository,
            InMemoryLeaderboardRepository>();

        services.AddSingleton<
            ISeasonRepository,
            InMemorySeasonRepository>();

        services.AddSingleton<
            IPlayerScoreRepository,
            InMemoryPlayerScoreRepository>();

        services.AddSingleton<
            ILeaderboardQueries,
            InMemoryLeaderboardQueries>();

        services.AddSingleton<
            IUnitOfWork,
            InMemoryUnitOfWork>();

        return services;
    }
}