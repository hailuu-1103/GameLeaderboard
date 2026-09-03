namespace GameLeaderboard.API.Contracts.Leaderboards;

public sealed record LeaderboardEntryResponse(
    int Rank,
    string PlayerName,
    long Score,
    DateTimeOffset AchievedAt
);
