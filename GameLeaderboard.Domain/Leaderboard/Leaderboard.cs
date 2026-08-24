namespace GameLeaderboard.Domain.Leaderboard;

using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Scores;

public sealed record Leaderboard
{
    public Leaderboard(LeaderboardId leaderboardId)
    {
        this.LeaderboardId = leaderboardId;
    }

    public LeaderboardId LeaderboardId { get; private set; }
}