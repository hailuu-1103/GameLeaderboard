namespace GameLeaderboard.Architecture.Tests;

using System.Reflection;
using GameLeaderboard.Application.Scores.SubmitScore;
using GameLeaderboard.Domain.Season;
using GameLeaderboard.Infrastructure.Persistence.InMemory;

internal static class Assemblies
{
    internal static readonly Assembly Domain =
        typeof(Season).Assembly;

    internal static readonly Assembly Application =
        typeof(SubmitScoreHandler).Assembly;

    internal static readonly Assembly Infrastructure =
        typeof(InMemoryLeaderboardRepository).Assembly;

    internal static readonly Assembly Api =
        typeof(Program).Assembly;

    internal const string DOMAIN_NAMESPACE =
        "GameLeaderboard.Domain";

    internal const string APPLICATION_NAMESPACE =
        "GameLeaderboard.Application";

    internal const string INFRASTRUCTURE_NAMESPACE =
        "GameLeaderboard.Infrastructure";

    internal const string API_NAMESPACE =
        "GameLeaderboard.Api";
}