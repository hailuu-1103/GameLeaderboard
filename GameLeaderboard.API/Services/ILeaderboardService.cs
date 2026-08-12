namespace GameLeaderboard.Api.Services;

public interface ILeaderboardService
{
    bool LeaderboardExists(string leaderboardId);

    IReadOnlyList<LeaderboardEntry> GetTop(
        string leaderboardId,
        int    limit
    );

    LeaderboardEntry? GetPlayer(
        string leaderboardId,
        string playerName
    );

    SubmitScoreResult SubmitScore(
        string leaderboardId,
        string playerName,
        long   score
    );
}