namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Season;

public class InMemoryLeaderboardQueries(InMemoryLeaderboardStore store) : ILeaderboardQueries
{
    public Task<IReadOnlyList<LeaderboardEntryModel>> GetTopBySeasonAsync(
        SeasonId          seasonId,
        int               limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.SyncRoot)
        {
            var result = store.PlayerScores
                .Where(entry =>
                    entry.Key.SeasonId == seasonId)
                .OrderByDescending(player =>
                    player.Value.BestScore.Value)
                .ThenBy(player => player.Value.AchievedAt)
                .ThenBy(
                    player => player.Key.PlayerId.Value,
                    StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .Select((player, index) =>
                    new LeaderboardEntryModel(
                        index + 1,
                        player.Value.PlayerName.Value,
                        player.Value.BestScore.Value,
                        player.Value.AchievedAt))
                .ToArray();
            return Task.FromResult<IReadOnlyList<LeaderboardEntryModel>>(result);
        }
    }

    public Task<LeaderboardEntryModel?> GetPlayerAsync(string leaderboardId, string playerName, CancellationToken cancellationToken)
    {
        lock (store.SyncRoot)
        {
            if (!store.Leaderboards.TryGetValue(
                leaderboardId,
                out var leaderboard))
            {
                return Task.FromResult<LeaderboardEntryModel?>(null);
            }

            var result = store.PlayerScores
                .OrderByDescending(player =>
                    player.Value.BestScore.Value)
                .ThenBy(player => player.Value.AchievedAt)
                .ThenBy(
                    player => player.Key.PlayerId.Value,
                    StringComparer.OrdinalIgnoreCase)
                .Select((player, index) =>
                    new LeaderboardEntryModel(
                        index + 1,
                        player.Value.PlayerName.Value,
                        player.Value.BestScore.Value,
                        player.Value.AchievedAt))
                .SingleOrDefault(kvp => kvp.PlayerName.Equals(playerName));
            return Task.FromResult<LeaderboardEntryModel?>(result);
        }
    }
}
