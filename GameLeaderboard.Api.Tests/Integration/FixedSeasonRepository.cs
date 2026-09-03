namespace GameLeaderboard.API.Tests.Integration;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Season;

public sealed class FixedSeasonRepository(Season season)
    : ISeasonRepository
{
    private int getCurrentCallCount;

    public int GetCurrentCallCount =>
        Volatile.Read(ref this.getCurrentCallCount);

    public Task<Season?> GetCurrentAsync(
        LeaderboardId     leaderboardId,
        DateTimeOffset    now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(
            ref this.getCurrentCallCount);

        return Task.FromResult<Season?>(season);
    }
}
