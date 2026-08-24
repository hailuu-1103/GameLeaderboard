namespace GameLeaderboard.Domain.Scores;

using GameLeaderboard.Domain.Season;

public sealed class PlayerScore
{
    public SeasonId   SeasonId   { get; }
    public PlayerId   PlayerId   { get; }
    public PlayerName PlayerName { get; }

    public Score          BestScore  { get; private set; }
    public DateTimeOffset AchievedAt { get; private set; }

    private PlayerScore(
        SeasonId       seasonId,
        PlayerId       playerId,
        PlayerName     playerName,
        Score          bestScore,
        DateTimeOffset achievedAt
    )
    {
        this.SeasonId   = seasonId;
        this.PlayerId   = playerId;
        this.PlayerName = playerName;
        this.BestScore  = bestScore;
        this.AchievedAt = achievedAt;
    }

    public static PlayerScore Create(
        SeasonId       seasonId,
        PlayerName     playerName,
        Score          firstScore,
        DateTimeOffset achievedAt
    )
    {
        return new(
            seasonId,
            PlayerId.From(playerName),
            playerName,
            firstScore,
            achievedAt);
    }

    public ScoreSubmissionDecision Submit(
        Score          submittedScore,
        DateTimeOffset submittedAt
    )
    {
        var previousBest = this.BestScore;

        if (submittedScore.Value <= this.BestScore.Value)
        {
            return new(
                submittedScore,
                previousBest,
                this.BestScore,
                false);
        }

        this.BestScore  = submittedScore;
        this.AchievedAt = submittedAt;

        return new(
            submittedScore,
            previousBest,
            this.BestScore,
            true);
    }
}