namespace GameLeaderboard.API.Contracts.Leaderboards;

public sealed record GetTopLeaderboardResponse(
    string LeaderboardId,
    string SeasonId,
    IReadOnlyList<LeaderboardEntryResponse> Entries
);
