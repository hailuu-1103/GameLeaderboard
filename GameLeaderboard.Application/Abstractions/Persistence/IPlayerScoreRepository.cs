using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

namespace GameLeaderboard.Application.Abstractions.Persistence;

public interface IPlayerScoreRepository
{
    public Task<PlayerScore?> GetAsync(
        SeasonId          seasonId,
        PlayerId          playerId,
        CancellationToken cancellationToken);

    public void Add(PlayerScore playerScore);
}