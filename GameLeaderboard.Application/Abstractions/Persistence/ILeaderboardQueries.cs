namespace GameLeaderboard.Application.Abstractions.Persistence;

using GameLeaderboard.Domain.Season;

public sealed record LeaderboardEntryModel(
    int            Rank,
    string         PlayerName,
    long           Score,
    DateTimeOffset AchievedAt);

public interface ILeaderboardQueries
{
    public Task<IReadOnlyList<LeaderboardEntryModel>>
        GetTopBySeasonAsync(
            SeasonId          seasonId,
            int               limit,
            CancellationToken cancellationToken);

    public Task<LeaderboardEntryModel?> GetPlayerAsync(
        string            leaderboardId,
        string            playerName,
        CancellationToken cancellationToken);
}
