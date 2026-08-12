namespace GameLeaderboard.Api.Services;

public sealed record LeaderboardEntry(
    int    Rank,
    string PlayerName,
    long   Score
);