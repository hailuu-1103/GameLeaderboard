namespace GameLeaderboard.Application.Leaderboards.GetTop;

public sealed record GetTopPlayerQuery(
    string LeaderboardId,
    int    Limit = 10
);