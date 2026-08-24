using GameLeaderboard.Domain.Scores;

public sealed record ScoreSubmissionDecision(
    Score SubmittedScore,
    Score PreviousBestScore,
    Score CurrentBestScore,
    bool  IsNewHighScore
);