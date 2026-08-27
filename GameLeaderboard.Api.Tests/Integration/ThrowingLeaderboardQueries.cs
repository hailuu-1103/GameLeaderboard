namespace GameLeaderboard.API.Tests.Integration;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Season;

public sealed class ThrowingLeaderboardQueries : ILeaderboardQueries
{
    public const string SENSITIVE_MESSAGE =
        "Sensitive leaderboard query failure.";

    public Task<IReadOnlyList<LeaderboardEntryModel>>
        GetTopBySeasonAsync(
            SeasonId          seasonId,
            int               limit,
            CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            SENSITIVE_MESSAGE);
    }

    public Task<LeaderboardEntryModel?> GetPlayerAsync(
        string            leaderboardId,
        string            playerName,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            SENSITIVE_MESSAGE);
    }
}
