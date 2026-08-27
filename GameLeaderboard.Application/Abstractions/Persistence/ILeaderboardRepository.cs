using GameLeaderboard.Domain.Leaderboard;

namespace GameLeaderboard.Application.Abstractions.Persistence;

public interface ILeaderboardRepository
{
    public Task<bool> ExistsAsync(
        LeaderboardId     leaderboardId,
        CancellationToken cancellationToken);
}