namespace GameLeaderboard.API.Contracts.Scores;

public sealed record SubmitScoreRequest(
    string PlayerName,
    long Score
);

public sealed record SubmitScoreResponse(
    string LeaderboardId,
    string PlayerName,
    long SubmittedScore,
    long? PreviousHighScore,
    bool IsNewHighScore
);
