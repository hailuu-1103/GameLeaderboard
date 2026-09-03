namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Season;

internal class InMemorySeasonRepository(InMemoryLeaderboardStore store) : ISeasonRepository
{
    public Task<Season?> GetCurrentAsync(
        LeaderboardId     leaderboardId,
        DateTimeOffset    now,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.SyncRoot)
        {
            var season = store.Seasons.Values
                .SingleOrDefault(season =>
                    string.Equals(
                        season.LeaderboardId.Value,
                        leaderboardId.Value,
                        StringComparison.OrdinalIgnoreCase) &&
                    season.AcceptsScoresAt(now));

            return Task.FromResult(season);
        }
    }
}