namespace GameLeaderboard.API.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GameLeaderboard.API.Contracts.Scores;
using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public sealed class LeaderboardScorePolicyApiTests
    : LeaderboardApiTestBase
{
    [Fact]
    public async Task SubmitScore_WhenSeasonIsScheduled_ReturnsSeasonNotActiveConflict()
    {
        var scheduledSeason = Season.Create(
            SeasonId.Create("scheduled-season"),
            LeaderboardId.Create("classic"),
            LeaderboardApiFactory.UtcNow.AddDays(-1),
            LeaderboardApiFactory.UtcNow.AddDays(1));

        var seasonRepository =
            new FixedSeasonRepository(scheduledSeason);

        await using var scheduledSeasonFactory =
            this.Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ISeasonRepository>();
                    services.AddSingleton<ISeasonRepository>(
                        seasonRepository);
                });
            });

        using var client   = CreateClient(scheduledSeasonFactory);
        using var response = await client.PostAsJsonAsync(
            "/api/leaderboards/classic/scores",
            new SubmitScoreRequest("David", 13_000));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:season-not-active",
            problem.Type);
        Assert.Equal("Score submission rejected.", problem.Title);
        Assert.Equal(
            "Scores can only be submitted to an active season.",
            problem.Detail);
        Assert.Equal(
            "/api/leaderboards/classic/scores",
            problem.Instance);
        AssertHasTraceId(problem);
        Assert.Equal(1, seasonRepository.GetCurrentCallCount);
    }

    [Fact]
    public async Task SubmitScore_WhenRepositoryReturnsWrongSeason_ReturnsSanitizedProblemDetails()
    {
        var wrongSeasonScore = PlayerScore.Create(
            SeasonId.Create("wrong-season"),
            PlayerName.Create("Alice"),
            Score.Create(12_000),
            LeaderboardApiFactory.UtcNow.AddDays(-1));

        var playerScoreRepository =
            new WrongSeasonPlayerScoreRepository(
                wrongSeasonScore);

        await using var wrongSeasonFactory =
            this.Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPlayerScoreRepository>();
                    services.AddSingleton<IPlayerScoreRepository>(
                        playerScoreRepository);
                });
            });

        using var client   = CreateClient(wrongSeasonFactory);
        using var response = await client.PostAsJsonAsync(
            "/api/leaderboards/classic/scores",
            new SubmitScoreRequest("Alice", 13_000));

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            "score-season-mismatch",
            responseJson);
        Assert.DoesNotContain(
            nameof(DomainRuleViolationException),
            responseJson);
        Assert.DoesNotContain(
            "The player score does not belong to this season.",
            responseJson);

        var problem = JsonSerializer.Deserialize<ProblemDetails>(
            responseJson,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:internal-server-error",
            problem.Type);
        Assert.Equal(
            "An unexpected error occurred.",
            problem.Title);
        Assert.Equal(
            "The server could not complete the request.",
            problem.Detail);
        Assert.Equal(
            "/api/leaderboards/classic/scores",
            problem.Instance);
        AssertHasTraceId(problem);
        Assert.Equal(1, playerScoreRepository.GetCallCount);
        Assert.Equal(0, playerScoreRepository.AddCallCount);
    }
}
