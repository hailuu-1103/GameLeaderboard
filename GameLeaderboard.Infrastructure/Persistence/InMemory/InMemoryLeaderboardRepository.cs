namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Leaderboard;

internal sealed class InMemoryLeaderboardRepository(
    InMemoryLeaderboardStore store)
    : ILeaderboardRepository
{
    public Task<bool> ExistsAsync(
        LeaderboardId     leaderboardId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.SyncRoot)
        {
            var exists =
                store.Leaderboards.ContainsKey(
                    leaderboardId.Value);

            return Task.FromResult(exists);
        }
    }
}