namespace GameLeaderboard.API.Tests.UnitTests.Scores.SubmitScore;

using GameLeaderboard.Application.Common;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

public sealed class SubmitScoreHandlerFailureTests
{
    [Fact]
    public async Task HandleAsync_WhenLeaderboardDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var fixture = new SubmitScoreHandlerFixture();
        fixture.LeaderboardRepository.Exists = false;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("leaderboard-not-found", result.Error.Code);
        Assert.Equal(
            "Leaderboard 'classic' does not exist.",
            result.Error.Description);
        Assert.Equal(ApplicationErrorType.NotFound, result.Error.Type);
        Assert.Equal(1, fixture.LeaderboardRepository.CallCount);
        Assert.Equal(0, fixture.SeasonRepository.CallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentSeasonDoesNotExist_ReturnsConflictWithoutSaving()
    {
        var fixture = new SubmitScoreHandlerFixture();
        fixture.SeasonRepository.Season = null;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("season-not-found", result.Error.Code);
        Assert.Equal(
            "No season is configured for score submission.",
            result.Error.Description);
        Assert.Equal(ApplicationErrorType.Conflict, result.Error.Type);
        Assert.Equal(1, fixture.SeasonRepository.CallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenSeasonIsInactive_ReturnsConflictWithoutAddingOrSaving()
    {
        var fixture = new SubmitScoreHandlerFixture();
        fixture.SeasonRepository.Season =
            SubmitScoreHandlerFixture.CreateScheduledSeason();

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("season-not-active", result.Error.Code);
        Assert.Equal(
            "Scores can only be submitted to an active season.",
            result.Error.Description);
        Assert.Equal(ApplicationErrorType.Conflict, result.Error.Type);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData("", "player-name-required", "Player name must contain a non-whitespace character.")]
    [InlineData("   ", "player-name-required", "Player name must contain a non-whitespace character.")]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "player-name-too-long", "Player name must not exceed 30 characters.")]
    public async Task HandleAsync_WhenPlayerNameIsInvalid_ReturnsValidationWithoutSaving(
        string playerName,
        string expectedCode,
        string expectedDescription)
    {
        var fixture = new SubmitScoreHandlerFixture();

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(playerName),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal(expectedDescription, result.Error.Description);
        Assert.Equal(ApplicationErrorType.Validation, result.Error.Type);
        Assert.Equal(0, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(1_000_000_001L)]
    public async Task HandleAsync_WhenScoreIsInvalid_ReturnsValidationWithoutSaving(
        long score)
    {
        var fixture = new SubmitScoreHandlerFixture();

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(score: score),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("score-out-of-range", result.Error.Code);
        Assert.Equal(
            "Score must be between 0 and 1000000000.",
            result.Error.Description);
        Assert.Equal(ApplicationErrorType.Validation, result.Error.Type);
        Assert.Equal(0, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsScoreFromDifferentSeason_PropagatesDomainException()
    {
        var fixture = new SubmitScoreHandlerFixture();
        fixture.PlayerScoreRepository.PlayerScore =
            SubmitScoreHandlerFixture.CreatePlayerScore(
                SeasonId.Create("season-2"),
                100,
                SubmitScoreHandlerFixture.UTC_NOW.AddHours(-1));

        var exception = await Assert.ThrowsAsync<DomainRuleViolationException>(
            () => fixture.CreateHandler().HandleAsync(
                SubmitScoreHandlerFixture.CreateCommand(score: 200),
                CancellationToken.None));

        Assert.Equal("score-season-mismatch", exception.Code);
        Assert.Equal(
            "The player score does not belong to this season.",
            exception.Message);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(0, fixture.UnitOfWork.SaveCallCount);
    }
}
