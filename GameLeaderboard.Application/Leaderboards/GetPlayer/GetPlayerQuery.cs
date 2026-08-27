namespace GameLeaderboard.Application.Leaderboards.GetPlayer;

public sealed record GetPlayerQuery(
    string LeaderboardId,
    string Name
);