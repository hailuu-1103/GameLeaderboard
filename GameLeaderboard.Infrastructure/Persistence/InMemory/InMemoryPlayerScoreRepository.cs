namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

internal class InMemoryPlayerScoreRepository(InMemoryLeaderboardStore store) : IPlayerScoreRepository
{
    public Task<PlayerScore?> GetAsync(SeasonId seasonId, PlayerId playerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.SyncRoot)
        {
            store.PlayerScores.TryGetValue(
                (seasonId, playerId),
                out var score);

            return Task.FromResult(score);
        }
    }

    public void Add(PlayerScore playerScore)
    {
        lock (store.SyncRoot)
        {
            store.PlayerScores.Add(
                (
                    playerScore.SeasonId,
                    playerScore.PlayerId
                ),
                playerScore);
        }
    }
}