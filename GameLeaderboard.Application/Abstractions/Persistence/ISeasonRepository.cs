using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Season;

namespace GameLeaderboard.Application.Abstractions.Persistence;

public interface ISeasonRepository
{
    public Task<Season?> GetCurrentAsync(
        LeaderboardId     leaderboardId,
        DateTimeOffset    now,
        CancellationToken cancellationToken);
}