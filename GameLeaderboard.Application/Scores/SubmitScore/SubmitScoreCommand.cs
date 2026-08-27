namespace GameLeaderboard.Application.Scores.SubmitScore;

public sealed record SubmitScoreCommand(
    string LeaderboardId,
    string PlayerName,
    long   Score);