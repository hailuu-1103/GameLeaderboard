namespace GameLeaderboard.Domain.Leaderboard;

using GameLeaderboard.Domain.Common;

public sealed record Leaderboard
{
    public LeaderboardId LeaderboardId { get; }

    private Leaderboard(LeaderboardId leaderboardId)
    {
        this.LeaderboardId = leaderboardId;
    }

    public static Leaderboard Create(LeaderboardId leaderboardId)
    {
        if (leaderboardId is null)
        {
            throw new DomainRuleViolationException(
                "leaderboard-id-required",
                "Leaderboard id is required.");
        }

        return new(leaderboardId);
    }
}
