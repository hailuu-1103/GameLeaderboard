namespace GameLeaderboard.API.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using GameLeaderboard.API.Contracts.Leaderboards;
using GameLeaderboard.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public sealed class LeaderboardTopApiTests
    : LeaderboardApiTestBase
{
    [Fact]
    public async Task GetTop_WhenRequestIsValid_ReturnsOrderedEntries()
    {
        using var response = await this.Client.GetAsync(
            "/api/leaderboards/classic/top?limit=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content
            .ReadFromJsonAsync<GetTopLeaderboardResponse>();

        Assert.NotNull(body);
        Assert.Equal("classic", body.LeaderboardId);
        Assert.Collection(
            body.Entries,
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

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetTop_WhenLimitIsInvalid_ReturnsValidationProblem(
        int limit)
    {
        using var response = await this.Client.GetAsync(
            $"/api/leaderboards/classic/top?limit={limit}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:limit-out-of-range",
            problem.Type);
        Assert.Equal(
            "Limit must be between 1 and 100.",
            problem.Detail);
        AssertHasTraceId(problem);
    }

    [Fact]
    public async Task GetTop_WhenLimitIsValid_ReturnsResults()
    {
        const int LIMIT = 2;
        using var response = await this.Client.GetAsync(
            $"/api/leaderboards/classic/top?limit={LIMIT}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content
            .ReadFromJsonAsync<GetTopLeaderboardResponse>();

        Assert.NotNull(body);
        Assert.Equal("classic", body.LeaderboardId);
        Assert.Equal(LIMIT, body.Entries.Count);
    }

    [Fact]
    public async Task GetTop_WhenQueryThrows_ReturnsSanitizedProblemDetails()
    {
        await using var failureFactory =
            this.Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ILeaderboardQueries>();
                    services.AddSingleton<
                        ILeaderboardQueries,
                        ThrowingLeaderboardQueries>();
                });
            });

        using var client   = CreateClient(failureFactory);
        using var response = await client.GetAsync(
            "/api/leaderboards/classic/top");

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            ThrowingLeaderboardQueries.SENSITIVE_MESSAGE,
            responseJson);
        Assert.DoesNotContain(
            nameof(InvalidOperationException),
            responseJson);

        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:internal-server-error",
            problem.Type);
        Assert.Equal(
            "An unexpected error occurred.",
            problem.Title);
        AssertHasTraceId(problem);
    }
}
