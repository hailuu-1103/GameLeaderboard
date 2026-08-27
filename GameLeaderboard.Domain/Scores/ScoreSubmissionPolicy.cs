namespace GameLeaderboard.Domain.Scores;

using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Season;

public sealed class ScoreSubmissionPolicy
{
    public PlayerScore CreateFirstScore(
        Season         season,
        PlayerName     playerName,
        Score          submittedScore,
        DateTimeOffset submittedAt)
    {
        EnsureSeasonAcceptsScore(
            season,
            submittedAt);

        return PlayerScore.Create(
            season.Id,
            playerName,
            submittedScore,
            submittedAt);
    }

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

        EnsureSeasonAcceptsScore(
            season,
            submittedAt);

        return playerScore.Submit(
            submittedScore,
            submittedAt);
    }

    private static void EnsureSeasonAcceptsScore(
        Season         season,
        DateTimeOffset submittedAt)
    {
        if (!season.AcceptsScoresAt(submittedAt))
        {
            throw new DomainRuleViolationException(
                "season-not-active",
                "Scores can only be submitted to an active season.");
        }
    }
}