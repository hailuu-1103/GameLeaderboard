namespace GameLeaderboard.API.Tests.Integration;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

public sealed class WrongSeasonPlayerScoreRepository(
    PlayerScore playerScore)
    : IPlayerScoreRepository
{
    private int addCallCount;
    private int getCallCount;

    public int AddCallCount =>
        Volatile.Read(ref this.addCallCount);

    public int GetCallCount =>
        Volatile.Read(ref this.getCallCount);

    public Task<PlayerScore?> GetAsync(
        SeasonId          seasonId,
        PlayerId          playerId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(
            ref this.getCallCount);

        return Task.FromResult<PlayerScore?>(playerScore);
    }

    public void Add(PlayerScore addedPlayerScore)
    {
        Interlocked.Increment(
            ref this.addCallCount);
    }
}
