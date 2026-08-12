namespace GameLeaderboard.Api.Services;

public sealed record SubmitScoreResult(
    long? PreviousHighScore,
    bool  IsNewHighScore
);