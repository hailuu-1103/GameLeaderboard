namespace GameLeaderboard.API.Tests.UnitTests;

using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;
using GameLeaderboard.Infrastructure.Persistence.InMemory;

public sealed class InMemoryLeaderboardTopQueryTests
{
    [Fact]
    public async Task GetTopBySeasonAsync_WhenScoresExist_ReturnsRankedEntries()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var entries = await queries.GetTopBySeasonAsync(
            SeasonId.Create("season-1"),
            10,
            CancellationToken.None);

        Assert.Collection(
            entries,
            first =>
            {
                Assert.Equal(1, first.Rank);
                Assert.Equal("Bob", first.PlayerName);
                Assert.Equal(20_000, first.Score);
                Assert.Equal(
                    DateTimeOffset.Parse("2026-08-21T12:00:00"),
                    first.AchievedAt);
            },
            second =>
            {
                Assert.Equal(2, second.Rank);
                Assert.Equal("Hai", second.PlayerName);
                Assert.Equal(15_000, second.Score);
                Assert.Equal(
                    DateTimeOffset.Parse("2026-08-21T10:00:00"),
                    second.AchievedAt);
            },
            third =>
            {
                Assert.Equal(3, third.Rank);
                Assert.Equal("Alice", third.PlayerName);
                Assert.Equal(12_000, third.Score);
                Assert.Equal(
                    DateTimeOffset.Parse("2026-08-21T11:00:00"),
                    third.AchievedAt);
            });
    }

    [Fact]
    public async Task GetTopBySeasonAsync_WhenScoresTie_UsesAchievedAtThenPlayerId()
    {
        var store      = new InMemoryLeaderboardStore();
        var repository = new InMemoryPlayerScoreRepository(store);
        var queries    = new InMemoryLeaderboardQueries(store);
        var seasonId   = SeasonId.Create("tie-season");
        var earlier    = DateTimeOffset.Parse("2026-08-23T10:00:00Z");
        var later      = DateTimeOffset.Parse("2026-08-23T11:00:00Z");

        repository.Add(PlayerScore.Create(
            seasonId,
            PlayerName.Create("Zulu"),
            Score.Create(100),
            later));

        repository.Add(PlayerScore.Create(
            seasonId,
            PlayerName.Create("Beta"),
            Score.Create(100),
            earlier));

        repository.Add(PlayerScore.Create(
            seasonId,
            PlayerName.Create("Alpha"),
            Score.Create(100),
            earlier));

        repository.Add(PlayerScore.Create(
            seasonId,
            PlayerName.Create("Highest"),
            Score.Create(200),
            later));

        // Act
        var entries = await queries.GetTopBySeasonAsync(
            seasonId,
            10,
            CancellationToken.None);

        // Assert
        Assert.Equal(
            ["Highest", "Alpha", "Beta", "Zulu"],
            entries.Select(entry => entry.PlayerName).ToArray());

        Assert.Equal(
            [1, 2, 3, 4],
            entries.Select(entry => entry.Rank).ToArray());
    }

    [Fact]
    public async Task GetTopBySeasonAsync_WhenPlayerIsAdded_RecalculatesRanking()
    {
        var store                 = new InMemoryLeaderboardStore();
        var playerScoreRepository = new InMemoryPlayerScoreRepository(store);
        var queries               = new InMemoryLeaderboardQueries(store);

        // Act
        playerScoreRepository.Add(PlayerScore.Create(
            SeasonId.Create("season-1"),
            PlayerName.Create("David"),
            Score.Create(13_000),
            DateTimeOffset.Parse("2026-08-23T10:00:00Z")));

        var entries = await queries.GetTopBySeasonAsync(SeasonId.Create("season-1"), 10, CancellationToken.None);

        // Assert
        Assert.Equal(
            ["Bob", "Hai", "David", "Alice"],
            entries.Select(entry => entry.PlayerName).ToArray());

        Assert.Equal(
            [1, 2, 3, 4],
            entries.Select(entry => entry.Rank).ToArray());
    }

    [Fact]
    public async Task GetTopBySeasonAsync_WhenLimitIsProvided_ReturnsOnlyRequestedNumberOfPlayers()
    {
        // Arrange
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        // Act
        var entries = await queries.GetTopBySeasonAsync(SeasonId.Create("season-1"), 2, CancellationToken.None);

        // Assert
        Assert.Equal(
            ["Bob", "Hai"],
            entries.Select(entry => entry.PlayerName).ToArray());
    }

    [Fact]
    public async Task GetTopBySeasonAsync_WhenSeasonHasNoScores_ReturnsEmptyCollection()
    {
        var store   = new InMemoryLeaderboardStore();
        var queries = new InMemoryLeaderboardQueries(store);

        var entries = await queries.GetTopBySeasonAsync(
            SeasonId.Create("season-without-scores"),
            10,
            CancellationToken.None);

        Assert.Empty(entries);
    }
}
