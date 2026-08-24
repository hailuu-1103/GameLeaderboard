namespace GameLeaderboard.Domain.Tests.UnitTests;

using Season = GameLeaderboard.Domain.Season.Season;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;
using Xunit;

public class DomainRuleTests
{
    #region Leaderboard

    [Fact]
    public void LeaderboardCreate_WhenIdIsNull_ThrowsDomainRuleViolationException()
    {
        var exception = Assert.Throws<DomainRuleViolationException>(
            () => Leaderboard.Create(null!));

        Assert.Equal("leaderboard-id-required", exception.Code);
    }

    #endregion

    #region PlayerName

    [Fact]
    public void PlayerNameCreate_WhenValueHasOuterWhitespace_ReturnsTrimmedName()
    {
        var name = PlayerName.Create(" Hai    ");
        Assert.Equal(PlayerName.Create("Hai"), name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void PlayerNameCreate_WhenValueIsEmptyOrWhitespace_ThrowsDomainRuleViolationException(string value)
    {
        // Act
        var exception = Assert.Throws<DomainRuleViolationException>(() => PlayerName.Create(value));

        // Assert
        Assert.Equal("player-name-required", exception.Code);
        Assert.Equal(
            "Player name must contain a non-whitespace character.",
            exception.Message);
    }

    [Theory]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void PlayerNameCreate_WhenValueExceedsMaximumLength_ThrowsDomainRuleViolationException(string value)
    {
        // Act
        var exception = Assert.Throws<DomainRuleViolationException>(() => PlayerName.Create(value));

        // Assert
        Assert.Equal("player-name-too-long", exception.Code);
        Assert.Equal(
            "Player name must not exceed 30 characters.",
            exception.Message);
    }

    #endregion

    #region PlayerId

    [Fact]
    public void PlayerIdFrom_WhenNamesDifferOnlyByCase_ReturnsSameId()
    {
        Assert.Equal(PlayerId.From(PlayerName.Create("David")), PlayerId.From(PlayerName.Create("david")));
    }

    #endregion

    #region Score

    [Theory]
    [InlineData(-1L)]
    [InlineData(1_000_000_001L)]
    public void ScoreCreate_WhenValueIsOutsideAllowedRange_ThrowsDomainRuleViolationException(long score)
    {
        // Act
        var exception = Assert.Throws<DomainRuleViolationException>(() => Score.Create(score));

        // Assert
        Assert.Equal("score-out-of-range", exception.Code);
        Assert.Equal(
            $"Score must be between {Score.MINIMUM} and {Score.MAXIMUM}.",
            exception.Message);
    }

    #endregion

    #region Season

    [Theory]
    [InlineData("2026-08-21T10:00:00", "2026-08-21T09:00:00")]
    [InlineData("2026-08-21T10:00:00", "2026-08-21T10:00:00")]
    public void SeasonCreate_WhenEndsAtIsNotAfterStartsAt_ThrowsDomainRuleViolationException(string startDateValue, string endDateValue)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateValue);
        var endDate = DateTime.Parse(endDateValue);

        // Act
        var exception = Assert.Throws<DomainRuleViolationException>(() => Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate));

        // Assert
        Assert.Equal("season-invalid-time", exception.Code);
        Assert.Equal(
            $"Start time must be smaller than end time",
            exception.Message);
    }

    [Theory]
    [InlineData("2026-08-21T10:00:00", "2026-08-25T09:00:00", "2026-08-22T09:00:00")]
    [InlineData("2026-08-21T10:00:00", "2027-08-21T10:00:00", "2026-08-25T09:00:00")]
    public void SeasonActivate_WhenCalledWithinSchedule_ChangesStatusToActive(string startDateValue, string endDateValue, string activeDateValue)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateValue);
        var endDate = DateTime.Parse(endDateValue);
        var activeDate = DateTime.Parse(activeDateValue);

        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        // Assert
        Assert.Equal(SeasonStatus.Active, season.Status);
    }

    [Theory]
    [InlineData("2026-08-21T10:00:00", "2026-08-25T09:00:00", "2026-08-20T09:00:00")]
    public void SeasonActivate_WhenCalledBeforeStartsAt_KeepsStatusScheduled(string startDateValue, string endDateValue, string activeDateValue)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateValue);
        var endDate = DateTime.Parse(endDateValue);
        var activeDate = DateTime.Parse(activeDateValue);

        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        // Assert
        Assert.Equal(SeasonStatus.Scheduled, season.Status);
    }

    [Theory]
    [InlineData("2026-08-21T10:00:00", "2026-08-25T09:00:00", "2026-08-22T09:00:00")]
    public void SeasonActivate_WhenSeasonIsClosed_ThrowsClosedSeasonReopeningRule(string startDateValue, string endDateValue, string activeDateValue)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateValue);
        var endDate = DateTime.Parse(endDateValue);
        var activeDate = DateTime.Parse(activeDateValue);

        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        season.Close(activeDate);

        var exception =
            Assert.Throws<DomainRuleViolationException>(() =>
                season.Activate(activeDate));

        Assert.Equal("closed-season-reopening-rule", exception.Code);
    }

    [Theory]
    [InlineData("2026-08-21T10:00:00", "2026-08-25T09:00:00", "2026-08-22T09:00:00")]
    public void SeasonActivate_WhenSeasonIsActive_ThrowsActiveSeasonReopeningRule(string startDateValue, string endDateValue, string activeDateValue)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateValue);
        var endDate = DateTime.Parse(endDateValue);
        var activeDate = DateTime.Parse(activeDateValue);

        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);

        var exception =
            Assert.Throws<DomainRuleViolationException>(() =>
                season.Activate(activeDate));

        Assert.Equal("active-season-reopening-rule", exception.Code);
    }

    [Fact]
    public void SeasonAcceptsScoresAt_WhenSeasonIsActiveAndTimeIsWithinSchedule_ReturnsTrue()
    {
        var startDate = DateTime.Parse("2026-08-21T10:00:00");
        var endDate = DateTime.Parse("2026-08-25T09:00:00");
        var activeDate = DateTime.Parse("2026-08-22T09:00:00");
        var acceptScoreDate = DateTime.Parse("2026-08-23T09:00:00");

        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        // Act
        var acceptsScore = season.AcceptsScoresAt(acceptScoreDate);

        // Assert
        Assert.True(acceptsScore);
    }

    [Fact]
    public void SeasonActivate_WhenNowEqualsEndsAt_KeepsStatusScheduled()
    {
        var season = Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            DateTimeOffset.Parse("2026-08-21T10:00:00Z"),
            DateTimeOffset.Parse("2026-08-25T10:00:00Z"));

        season.Activate(season.EndsAt);

        Assert.Equal(SeasonStatus.Scheduled, season.Status);
    }

    [Fact]
    public void SeasonClose_WhenSeasonIsScheduled_ThrowsSeasonNotActive()
    {
        var startsAt = DateTimeOffset.Parse("2026-08-21T10:00:00Z");
        var season = Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            startsAt,
            DateTimeOffset.Parse("2026-08-25T10:00:00Z"));

        var exception = Assert.Throws<DomainRuleViolationException>(
            () => season.Close(startsAt));

        Assert.Equal("season-not-active", exception.Code);
    }

    #endregion

    #region PlayerScore

    [Fact]
    public void PlayerScoreSubmit_WhenScoreIsHigher_UpdatesBestScore()
    {
        var startDate = DateTime.Parse("2026-08-21T10:00:00");
        var endDate = DateTime.Parse("2026-08-25T09:00:00");
        var activeDate = DateTime.Parse("2026-08-21T09:00:00");
        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        var playerScore = PlayerScore.Create(season.Id, PlayerName.Create("Hai"), Score.Create(10), DateTime.Parse("2026-08-21T10:00:00"));
        var submitScore = Score.Create(15);
        // Assert
        playerScore.Submit(submitScore, DateTime.Parse("2026-08-21T11:00:00"));
        Assert.Equal(playerScore.BestScore, submitScore);
    }

    [Fact]
    public void PlayerScoreSubmit_WhenScoreIsLower_KeepsBestScoreAndAchievedAt()
    {
        var startDate = DateTime.Parse("2026-08-21T10:00:00");
        var endDate = DateTime.Parse("2026-08-25T09:00:00");
        var activeDate = DateTime.Parse("2026-08-21T09:00:00");
        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        var currentScore = Score.Create(10);
        var archivedAt = DateTime.Parse("2026-08-21T10:00:00");
        var playerScore = PlayerScore.Create(season.Id, PlayerName.Create("Hai"), currentScore, archivedAt);
        var submitScore = Score.Create(9);
        // Assert
        playerScore.Submit(submitScore, DateTime.Parse("2026-08-21T11:00:00"));
        Assert.Equal(playerScore.BestScore, currentScore);
        Assert.Equal(playerScore.AchievedAt, archivedAt);
    }

    [Fact]
    public void PlayerScoreSubmit_WhenScoreIsEqual_KeepsAchievedAt()
    {
        var startDate = DateTime.Parse("2026-08-21T10:00:00");
        var endDate = DateTime.Parse("2026-08-25T09:00:00");
        var activeDate = DateTime.Parse("2026-08-21T09:00:00");
        var season = Season.Create(SeasonId.Create("season-1"), LeaderboardId.Create("leaderboard-1"), startDate, endDate);
        season.Activate(activeDate);
        var currentScore = Score.Create(10);
        var archiveAt = DateTime.Parse("2026-08-21T10:00:00");
        var playerScore = PlayerScore.Create(season.Id, PlayerName.Create("Hai"), currentScore, archiveAt);
        var submitScore = Score.Create(10);
        playerScore.Submit(submitScore, DateTime.Parse("2026-08-21T11:00:00"));
        Assert.Equal(playerScore.AchievedAt, archiveAt);
    }

    #endregion

    #region ScoreSubmissionPolicy

    [Fact]
    public void ScoreSubmissionPolicySubmit_WhenSeasonIsInactive_ThrowsSeasonNotActive()
    {
        var start = DateTimeOffset.Parse(
            "2026-08-21T10:00:00Z");

        var end = DateTimeOffset.Parse(
            "2026-08-25T10:00:00Z");

        var submittedAt = DateTimeOffset.Parse(
            "2026-08-22T10:00:00Z");

        var season = Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            start,
            end);

        var playerScore = PlayerScore.Create(
            SeasonId.Create("season-1"),
            PlayerName.Create("Hai"),
            Score.Create(100),
            submittedAt);

        var sut = new ScoreSubmissionPolicy();

        var exception =
            Assert.Throws<DomainRuleViolationException>(() =>
                sut.Submit(
                    season,
                    playerScore,
                    Score.Create(200),
                    submittedAt));

        Assert.Equal("season-not-active", exception.Code);
    }

    [Fact]
    public void ScoreSubmissionPolicySubmit_WhenSeasonIdsDoNotMatch_ThrowsScoreSeasonMismatch()
    {
        var start = DateTimeOffset.Parse(
            "2026-08-21T10:00:00Z");

        var end = DateTimeOffset.Parse(
            "2026-08-25T10:00:00Z");

        var submittedAt = DateTimeOffset.Parse(
            "2026-08-22T10:00:00Z");

        var season = Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            start,
            end);

        var playerScore = PlayerScore.Create(
            SeasonId.Create("season-2"),
            PlayerName.Create("Hai"),
            Score.Create(100),
            submittedAt);

        var sut = new ScoreSubmissionPolicy();

        var exception =
            Assert.Throws<DomainRuleViolationException>(() =>
                sut.Submit(
                    season,
                    playerScore,
                    Score.Create(200),
                    submittedAt));

        Assert.Equal("score-season-mismatch", exception.Code);
    }

    #endregion
}
