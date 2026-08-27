namespace GameLeaderboard.Application.Scores.SubmitScore;

public sealed record SubmitScoreOutput(
    string LeaderboardId,
    string PlayerName,
    long   SubmittedScore,
    long?  PreviousHighScore,
    bool   IsNewHighScore);