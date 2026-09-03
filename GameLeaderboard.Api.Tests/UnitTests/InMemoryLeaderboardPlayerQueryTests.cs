namespace GameLeaderboard.API.Tests.UnitTests;

using GameLeaderboard.Infrastructure.Persistence.InMemory;

public sealed class InMemoryLeaderboardPlayerQueryTests
{
    [Fact]
    public async Task GetPlayer_WhenPlayerNameDiffersOnlyByCase_ReturnsPlayer()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var result = await queries.GetPlayerAsync(
            "classic",
            "hAi",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Rank);
        Assert.Equal("Hai", result.PlayerName);
        Assert.Equal(15_000, result.Score);
        Assert.Equal(
            DateTimeOffset.Parse("2026-08-21T10:00:00"),
            result.AchievedAt);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerNotFound_ReturnsNull()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var result = await queries.GetPlayerAsync(
            "classic",
            "Unknown",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayer_WhenLeaderboardNotFound_ReturnsNull()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var result = await queries.GetPlayerAsync(
            "unknown",
            "Hai",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPlayerAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            queries.GetPlayerAsync(
                "classic",
                "Hai",
                cancellationSource.Token));
    }
}
