namespace GameLeaderboard.Application.Leaderboards.GetTop;

public sealed record LeaderboardEntryOutput(
    int            Rank,
    string         PlayerName,
    long           Score,
    DateTimeOffset AchievedAt);

public sealed record GetTopPlayerOutput(
    string                                LeaderboardId,
    string                                SeasonId,
    IReadOnlyList<LeaderboardEntryOutput> Entries);