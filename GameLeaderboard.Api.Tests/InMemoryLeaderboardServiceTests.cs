using GameLeaderboard.Api.Services;

namespace GameLeaderboard.Api.Tests.Services;

public sealed class InMemoryLeaderboardServiceTests
{
    #region Leaderboard Existence

    [Fact]
    public void LeaderboardExists_WhenLeaderboardExists_ReturnsTrue()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.LeaderboardExists("classic");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void LeaderboardExists_WhenLeaderboardDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.LeaderboardExists("unknown");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Get Top

    [Fact]
    public void GetTop_WhenCalled_ReturnsPlayersOrderedByDescendingScore()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var entries = service.GetTop("classic", 10);

        // Assert
        Assert.Equal(3, entries.Count);

        Assert.Equal("Hai", entries[0].PlayerName);
        Assert.Equal(15_000, entries[0].Score);
        Assert.Equal(1, entries[0].Rank);

        Assert.Equal("Alice", entries[1].PlayerName);
        Assert.Equal(12_000, entries[1].Score);
        Assert.Equal(2, entries[1].Rank);

        Assert.Equal("Bob", entries[2].PlayerName);
        Assert.Equal(9_500, entries[2].Score);
        Assert.Equal(3, entries[2].Rank);
    }

    [Fact]
    public void GetTop_WhenPlayerIsAdded_RecalculatesRanking()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        service.SubmitScore(
            "classic",
            "David",
            13_000);
        var entries = service.GetTop("classic", 10);

        // Assert
        Assert.Equal(
            ["Hai", "David", "Alice", "Bob"],
            entries.Select(entry => entry.PlayerName).ToArray());

        Assert.Equal(
            [1, 2, 3, 4],
            entries.Select(entry => entry.Rank).ToArray());
    }

    [Fact]
    public void GetTop_WhenLimitIsProvided_ReturnsOnlyRequestedNumberOfPlayers()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var entries = service.GetTop("classic", 2);

        // Assert
        Assert.Equal(
            ["Hai", "Alice"],
            entries.Select(entry => entry.PlayerName).ToArray());
    }

    [Fact]
    public void GetTop_WhenLeaderboardDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var entries = service.GetTop("unknown", 10);

        // Assert
        Assert.Empty(entries);
    }

    #endregion

    #region Submit Score

    [Fact]
    public void SubmitScore_WhenPlayerIsNew_AddsPlayer()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.SubmitScore(
            "classic",
            "David",
            13_000);

        var player = service.GetPlayer(
            "classic",
            "David");

        // Assert
        Assert.True(result.IsNewHighScore);
        Assert.Null(result.PreviousHighScore);

        Assert.NotNull(player);
        Assert.Equal("David", player.PlayerName);
        Assert.Equal(13_000, player.Score);
        Assert.Equal(2, player.Rank);
    }

    [Fact]
    public void SubmitScore_WhenScoreEqualsHighScore_DoesNotUpdateHighScore()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.SubmitScore(
            "classic",
            "Alice",
            12_000);

        var player = service.GetPlayer(
            "classic",
            "Alice");

        // Assert
        Assert.False(result.IsNewHighScore);
        Assert.Equal(12_000, result.PreviousHighScore);

        Assert.NotNull(player);
        Assert.Equal(12_000, player.Score);
    }

    [Fact]
    public void SubmitScore_WhenScoreIsHigher_UpdatesHighScore()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.SubmitScore(
            "classic",
            "Alice",
            18_000);

        var player = service.GetPlayer(
            "classic",
            "Alice");

        // Assert
        Assert.True(result.IsNewHighScore);
        Assert.Equal(12_000, result.PreviousHighScore);

        Assert.NotNull(player);
        Assert.Equal(18_000, player.Score);
        Assert.Equal(1, player.Rank);
    }

    [Fact]
    public void SubmitScore_WhenScoreIsLower_DoesNotUpdateHighScore()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        var result = service.SubmitScore(
            "classic",
            "Alice",
            10_000);

        var player = service.GetPlayer(
            "classic",
            "Alice");

        // Assert
        Assert.False(result.IsNewHighScore);
        Assert.Equal(12_000, result.PreviousHighScore);

        Assert.NotNull(player);
        Assert.Equal(12_000, player.Score);
    }

    #endregion

    #region Case-Insensitive Operations

    [Fact]
    public void Operations_WhenIdentifiersDifferOnlyByCase_AreCaseInsensitive()
    {
        // Arrange
        var service = new InMemoryLeaderboardService();

        // Act
        service.SubmitScore(
            "classic",
            "ALICE",
            18_000);
        var leaderboardExists = service.LeaderboardExists("CLASSIC");
        var player = service.GetPlayer("CLASSIC", "alice");
        var entries = service.GetTop("classic", 10);

        // Assert
        Assert.True(leaderboardExists);
        Assert.NotNull(player);
        Assert.Equal(18_000, player.Score);
        Assert.Equal(3, entries.Count);
    }

    #endregion
}
