using GameLeaderboard.Api.Services;

namespace GameLeaderboard.Api.Tests.Integration;

internal sealed class ThrowingLeaderboardService
    : ILeaderboardService
{
    public const string SENSITIVE_MESSAGE =
        "Database password is secret123.";

    public bool LeaderboardExists(string leaderboardId)
    {
        throw new InvalidOperationException(
            SENSITIVE_MESSAGE);
    }

    public IReadOnlyList<LeaderboardEntry> GetTop(
        string leaderboardId,
        int    limit)
    {
        throw new NotSupportedException();
    }

    public LeaderboardEntry? GetPlayer(
        string leaderboardId,
        string playerName)
    {
        throw new NotSupportedException();
    }

    public SubmitScoreResult SubmitScore(
        string leaderboardId,
        string playerName,
        long   score)
    {
        throw new NotSupportedException();
    }
}