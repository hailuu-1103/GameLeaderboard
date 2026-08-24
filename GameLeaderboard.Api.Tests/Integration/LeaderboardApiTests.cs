using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GameLeaderboard.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using GameLeaderboard.Api.Controllers;
using GameLeaderboard.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public sealed class LeaderboardsApiTests : IDisposable
{
    private readonly LeaderboardApiFactory factory;
    private readonly HttpClient client;

    public LeaderboardsApiTests()
    {
        this.factory = new LeaderboardApiFactory();

        this.client = this.factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false,
            });
    }

    #region Get Top

    [Fact]
    public async Task GetTop_WhenRequestIsValid_ReturnsOrderedEntries()
    {
        // Act
        using var response = await this.client.GetAsync(
            "/api/leaderboards/classic/top?limit=2");

        // Assert: HTTP contract
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        // Assert: JSON contract
        var body =
            await response.Content
                .ReadFromJsonAsync<TopLeaderboardResponse>();

        Assert.NotNull(body);
        Assert.Equal("classic", body.LeaderboardId);
        Assert.Equal(2, body.Entries.Count);

        Assert.Equal("Hai", body.Entries[0].PlayerName);
        Assert.Equal(15_000, body.Entries[0].Score);
        Assert.Equal(1, body.Entries[0].Rank);

        Assert.Equal("Alice", body.Entries[1].PlayerName);
        Assert.Equal(12_000, body.Entries[1].Score);
        Assert.Equal(2, body.Entries[1].Rank);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetTop_WhenLimitIsInvalid_ReturnsValidationProblem(
        int limit)
    {
        using var response = await this.client.GetAsync(
            $"/api/leaderboards/classic/top?limit={limit}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("limit", problem.Errors);
        Assert.Contains(
            "Limit must be between 1 and 100.",
            problem.Errors["limit"]);

        Assert.True(
            problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task GetTop_WhenLimitIsValid_ReturnsResults()
    {
        const int LIMIT = 2;
        using var response = await this.client.GetAsync(
            $"/api/leaderboards/classic/top?limit={LIMIT}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var body =
            await response.Content
                .ReadFromJsonAsync<TopLeaderboardResponse>();

        Assert.NotNull(body);
        Assert.Equal("classic", body.LeaderboardId);
        Assert.Equal(LIMIT, body.Entries.Count);
    }

    [Fact]
    public async Task GetTop_WhenServiceThrows_ReturnsSanitizedProblemDetails()
    {
        await using var failureFactory =
            this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<ILeaderboardService>();

                    services.AddSingleton<
                        ILeaderboardService,
                        ThrowingLeaderboardService>();
                });
            });

        using var client = failureFactory.CreateClient(
            new()
            {
                BaseAddress = new("https://localhost"),
                AllowAutoRedirect = false,
            });

        using var response = await client.GetAsync(
            "/api/leaderboards/classic/top");

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(
            ThrowingLeaderboardService.SENSITIVE_MESSAGE,
            responseJson);

        Assert.DoesNotContain(
            nameof(InvalidOperationException),
            responseJson);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal(
            "urn:game-leaderboard:errors:internal-server-error",
            problem.Type);
        Assert.Equal(
            "An unexpected error occurred.",
            problem.Title);
        Assert.True(
            problem.Extensions.ContainsKey("traceId"));
    }

    #endregion

    #region Get Player

    [Fact]
    public async Task GetPlayer_WhenPlayerDoesNotExist_ReturnsNotFoundProblem()
    {
        using var response = await this.client.GetAsync(
            "/api/leaderboards/classic/player?name=unknown");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.True(
            problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerNameIsEmpty_ReturnsNotFoundProblem()
    {
        using var response = await this.client.GetAsync(
            "/api/leaderboards/classic/player?name=");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.True(
            problem.Extensions.ContainsKey("traceId"));
    }

    #endregion

    #region Submit Score

    [Fact]
    public async Task SubmitScore_WhenPlayerIsNew_CanBeRetrievedAfterward()
    {
        var request = new SubmitScoreRequest(
            "David",
            13_000);

        using var submitResponse =
            await this.client.PostAsJsonAsync(
                "/api/leaderboards/classic/scores",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            submitResponse.StatusCode);

        using var getResponse = await this.client.GetAsync(
            "/api/leaderboards/classic/player/David");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var player =
            await getResponse.Content
                .ReadFromJsonAsync<PlayerResponse>();

        Assert.NotNull(player);
        Assert.Equal("David", player.PlayerName);
        Assert.Equal(13_000, player.Score);
        Assert.Equal(2, player.Rank);
    }

    [Fact]
    public async Task SubmitScore_WhenJsonIsMalformed_ReturnsBadRequest()
    {
        var malformedJson = """
                            {
                              "playerName": "David",
                              "score": 13000
                            """;

        using var content = new StringContent(
            malformedJson,
            Encoding.UTF8,
            "application/json");

        using var response = await this.client.PostAsync(
            "/api/leaderboards/classic/scores",
            content);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(
            problem.Extensions.ContainsKey("traceId"));
    }

    #endregion

    public void Dispose()
    {
        this.client.Dispose();
        this.factory.Dispose();
    }
}
