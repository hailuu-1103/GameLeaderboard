namespace GameLeaderboard.API.Tests.UnitTests;

using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Infrastructure.Persistence.InMemory;

public sealed class InMemoryLeaderboardRepositoryTests
{
    [Fact]
    public async Task LeaderboardExists_WhenLeaderboardExists_ReturnsTrue()
    {
        var store      = new InMemoryLeaderboardStore();
        var repository = new InMemoryLeaderboardRepository(store);

        // Act
        var result = await repository.ExistsAsync(LeaderboardId.Create("classic"), CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LeaderboardExists_WhenLeaderboardDoesNotExist_ReturnsFalse()
    {
        var store      = new InMemoryLeaderboardStore();
        var repository = new InMemoryLeaderboardRepository(store);

        // Act
        var result = await repository.ExistsAsync(LeaderboardId.Create("unknown"), CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}
