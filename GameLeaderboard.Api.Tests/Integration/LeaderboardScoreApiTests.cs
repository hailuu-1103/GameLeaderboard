namespace GameLeaderboard.API.Tests.Integration;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using GameLeaderboard.API.Contracts.Leaderboards;
using GameLeaderboard.API.Contracts.Scores;
using Microsoft.AspNetCore.Mvc;

public sealed class LeaderboardScoreApiTests
    : LeaderboardApiTestBase
{
    [Fact]
    public async Task SubmitScore_WhenPlayerIsNew_CanBeRetrievedAfterward()
    {
        using var submitResponse =
            await this.Client.PostAsJsonAsync(
                "/api/leaderboards/classic/scores",
                new SubmitScoreRequest("David", 13_000));

        Assert.Equal(
            HttpStatusCode.OK,
            submitResponse.StatusCode);

        using var getResponse = await this.Client.GetAsync(
            "/api/leaderboards/classic/top");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var leaderboard = await getResponse.Content
            .ReadFromJsonAsync<GetTopLeaderboardResponse>();

        Assert.NotNull(leaderboard);
        var player = Assert.Single(
            leaderboard.Entries,
            entry => entry.PlayerName == "David");
        Assert.Equal("David", player.PlayerName);
        Assert.Equal(13_000, player.Score);
        Assert.Equal(3, player.Rank);
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

        using var response = await this.Client.PostAsync(
            "/api/leaderboards/classic/scores",
            content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        AssertHasTraceId(problem);
    }
}
