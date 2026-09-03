namespace GameLeaderboard.API.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using GameLeaderboard.API.Contracts.Leaderboards;
using Microsoft.AspNetCore.Mvc;

public sealed class LeaderboardPlayerApiTests
    : LeaderboardApiTestBase
{
    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ReturnsNotFoundProblem()
    {
        const string PATH =
            "/api/leaderboards/classic/player/unknown";

        using var response = await this.Client.GetAsync(PATH);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:player-not-found",
            problem.Type);
        Assert.Equal("Player not found.", problem.Title);
        Assert.Equal(
            "Player 'unknown' does not exist in leaderboard 'classic'.",
            problem.Detail);
        Assert.Equal(PATH, problem.Instance);
        AssertHasTraceId(problem);
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerExists_ReturnsPlayer()
    {
        using var response = await this.Client.GetAsync(
            "/api/leaderboards/classic/player/Alice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var player = await response.Content
            .ReadFromJsonAsync<LeaderboardEntryResponse>();

        Assert.NotNull(player);
        Assert.Equal(3, player.Rank);
        Assert.Equal("Alice", player.PlayerName);
        Assert.Equal(12_000, player.Score);
    }

    [Fact]
    public async Task GetPlayer_WhenNameUsesDifferentCasing_ReturnsCanonicalPlayer()
    {
        using var response = await this.Client.GetAsync(
            "/api/leaderboards/classic/player/alice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var player = await response.Content
            .ReadFromJsonAsync<LeaderboardEntryResponse>();

        Assert.NotNull(player);
        Assert.Equal(3, player.Rank);
        Assert.Equal("Alice", player.PlayerName);
        Assert.Equal(12_000, player.Score);
    }
}
