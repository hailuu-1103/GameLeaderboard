namespace GameLeaderboard.API.Tests.UnitTests.Scores.SubmitScore;

using GameLeaderboard.Domain.Scores;

public sealed class SubmitScoreHandlerSubmissionTests
{
    [Fact]
    public async Task HandleAsync_WhenPlayerIsNew_AddsScoreAndSavesOnce()
    {
        var fixture = new SubmitScoreHandlerFixture();
        using var cancellationSource = new CancellationTokenSource();
        var cancellationToken = cancellationSource.Token;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand(),
            cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("classic", result.Value.LeaderboardId);
        Assert.Equal("David", result.Value.PlayerName);
        Assert.Equal(13_000, result.Value.SubmittedScore);
        Assert.Null(result.Value.PreviousHighScore);
        Assert.True(result.Value.IsNewHighScore);
        Assert.Equal(1, fixture.LeaderboardRepository.CallCount);
        Assert.Equal(1, fixture.SeasonRepository.CallCount);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(1, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveCallCount);

        var added = fixture.PlayerScoreRepository.AddedPlayerScore;
        Assert.NotNull(added);
        Assert.Equal(fixture.SeasonRepository.Season!.Id, added!.SeasonId);
        Assert.Equal(PlayerName.Create("David"), added.PlayerName);
        Assert.Equal(Score.Create(13_000), added.BestScore);
        Assert.Equal(SubmitScoreHandlerFixture.UTC_NOW, added.AchievedAt);
        Assert.Equal("classic", fixture.LeaderboardRepository.ReceivedLeaderboardId!.Value);
        Assert.Equal("classic", fixture.SeasonRepository.ReceivedLeaderboardId!.Value);
        Assert.Equal(added.SeasonId, fixture.PlayerScoreRepository.ReceivedSeasonId);
        Assert.Equal(added.PlayerId, fixture.PlayerScoreRepository.ReceivedPlayerId);
        Assert.Equal(cancellationToken, fixture.LeaderboardRepository.ReceivedCancellationToken);
        Assert.Equal(cancellationToken, fixture.SeasonRepository.ReceivedCancellationToken);
        Assert.Equal(cancellationToken, fixture.PlayerScoreRepository.ReceivedCancellationToken);
        Assert.Equal(cancellationToken, fixture.UnitOfWork.ReceivedCancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WhenScoreIsHigher_UpdatesScoreAndAchievedAt()
    {
        var fixture = new SubmitScoreHandlerFixture();
        var previousAchievedAt = SubmitScoreHandlerFixture.UTC_NOW.AddHours(-1);
        var playerScore = SubmitScoreHandlerFixture.CreatePlayerScore(
            fixture.SeasonRepository.Season!.Id,
            100,
            previousAchievedAt);
        fixture.PlayerScoreRepository.PlayerScore = playerScore;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand("Alice", 200),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.PreviousHighScore);
        Assert.True(result.Value.IsNewHighScore);
        Assert.Equal(Score.Create(200), playerScore.BestScore);
        Assert.Equal(SubmitScoreHandlerFixture.UTC_NOW, playerScore.AchievedAt);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenScoreIsLower_KeepsBestScoreAndAchievedAt()
    {
        var fixture = new SubmitScoreHandlerFixture();
        var previousAchievedAt = SubmitScoreHandlerFixture.UTC_NOW.AddHours(-1);
        var playerScore = SubmitScoreHandlerFixture.CreatePlayerScore(
            fixture.SeasonRepository.Season!.Id,
            100,
            previousAchievedAt);
        fixture.PlayerScoreRepository.PlayerScore = playerScore;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand("Alice", 99),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.PreviousHighScore);
        Assert.False(result.Value.IsNewHighScore);
        Assert.Equal(Score.Create(100), playerScore.BestScore);
        Assert.Equal(previousAchievedAt, playerScore.AchievedAt);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenScoreIsEqual_KeepsAchievedAt()
    {
        var fixture = new SubmitScoreHandlerFixture();
        var previousAchievedAt = SubmitScoreHandlerFixture.UTC_NOW.AddHours(-1);
        var playerScore = SubmitScoreHandlerFixture.CreatePlayerScore(
            fixture.SeasonRepository.Season!.Id,
            100,
            previousAchievedAt);
        fixture.PlayerScoreRepository.PlayerScore = playerScore;

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand("Alice", 100),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.PreviousHighScore);
        Assert.False(result.Value.IsNewHighScore);
        Assert.Equal(previousAchievedAt, playerScore.AchievedAt);
        Assert.Equal(1, fixture.PlayerScoreRepository.GetCallCount);
        Assert.Equal(0, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenUsingFixedClock_PassesExactTimeToSeasonAndPolicy()
    {
        var fixture = new SubmitScoreHandlerFixture();

        var result = await fixture.CreateHandler().HandleAsync(
            SubmitScoreHandlerFixture.CreateCommand("ClockedPlayer"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            SubmitScoreHandlerFixture.UTC_NOW,
            fixture.SeasonRepository.ReceivedNow);
        Assert.NotNull(fixture.PlayerScoreRepository.AddedPlayerScore);
        Assert.Equal(
            SubmitScoreHandlerFixture.UTC_NOW,
            fixture.PlayerScoreRepository.AddedPlayerScore!.AchievedAt);
        Assert.Equal(1, fixture.PlayerScoreRepository.AddCallCount);
        Assert.Equal(1, fixture.UnitOfWork.SaveCallCount);
    }
}
