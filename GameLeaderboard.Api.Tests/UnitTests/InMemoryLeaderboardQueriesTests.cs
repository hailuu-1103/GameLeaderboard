namespace GameLeaderboard.API.Tests.UnitTests;

using GameLeaderboard.Domain.Season;
using GameLeaderboard.Infrastructure.Persistence.InMemory;

public sealed class InMemoryLeaderboardQueriesTests
{


    [Fact]
    public async Task GetTopBySeasonAsync_WhenSeasonExists_ReturnsRankedEntries()
    {
        var store = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var entries = await queries.GetTopBySeasonAsync(
            SeasonId.Create("season-1"),
            2,
            CancellationToken.None);

        Assert.Collection(
            entries,
            first =>
            {
                Assert.Equal(1, first.Rank);
                Assert.Equal("Bob", first.PlayerName);
                Assert.Equal(20_000, first.Score);
            },
            second =>
            {
                Assert.Equal(2, second.Rank);
                Assert.Equal("Hai", second.PlayerName);
                Assert.Equal(15_000, second.Score);
            });
    }

    [Fact]
    public async Task GetTopBySeasonAsync_WhenSeasonHasNoScores_ReturnsEmptyCollection()
    {
        var store = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var entries = await queries.GetTopBySeasonAsync(
            SeasonId.Create("season-without-scores"),
            10,
            CancellationToken.None);

        Assert.Empty(entries);
    }
}