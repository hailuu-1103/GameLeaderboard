namespace GameLeaderboard.Domain.Tests.UnitTests;

using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

public sealed class ScoreSubmissionPolicyTests
{
    private static readonly DateTimeOffset SUBMITTED_AT =
        new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateFirstScore_WhenSeasonIsActive_CreatesPlayerScore()
    {
        var season = CreateScheduledSeason();
        season.Activate(SUBMITTED_AT);
        var policy = new ScoreSubmissionPolicy();

        var playerScore = policy.CreateFirstScore(
            season,
            PlayerName.Create("David"),
            Score.Create(13_000),
            SUBMITTED_AT);

        Assert.Equal(season.Id, playerScore.SeasonId);
        Assert.Equal(PlayerName.Create("David"), playerScore.PlayerName);
        Assert.Equal(Score.Create(13_000), playerScore.BestScore);
        Assert.Equal(SUBMITTED_AT, playerScore.AchievedAt);
    }

    [Fact]
    public void CreateFirstScore_WhenSeasonIsScheduled_ThrowsSeasonNotActive()
    {
        var policy = new ScoreSubmissionPolicy();

        var exception = Assert.Throws<DomainRuleViolationException>(() =>
            policy.CreateFirstScore(
                CreateScheduledSeason(),
                PlayerName.Create("David"),
                Score.Create(13_000),
                SUBMITTED_AT));

        Assert.Equal("season-not-active", exception.Code);
        Assert.Equal(
            "Scores can only be submitted to an active season.",
            exception.Message);
    }

    private static Season CreateScheduledSeason()
    {
        return Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            SUBMITTED_AT.AddDays(-1),
            SUBMITTED_AT.AddDays(1));
    }
}
