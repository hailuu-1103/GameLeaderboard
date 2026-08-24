using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

public sealed class ScoreSubmissionPolicy
{
    public ScoreSubmissionDecision Submit(
        Season         season,
        PlayerScore    playerScore,
        Score          submittedScore,
        DateTimeOffset submittedAt)
    {
        if (season.Id != playerScore.SeasonId)
        {
            throw new DomainRuleViolationException(
                "score-season-mismatch",
                "The player score does not belong to this season.");
        }

        if (!season.AcceptsScoresAt(submittedAt))
        {
            throw new DomainRuleViolationException(
                "season-not-active",
                "Scores can only be submitted to an active season.");
        }

        return playerScore.Submit(
            submittedScore,
            submittedAt);
    }
}