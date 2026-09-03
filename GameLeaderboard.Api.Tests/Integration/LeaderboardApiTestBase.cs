namespace GameLeaderboard.API.Tests.Integration;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

public abstract class LeaderboardApiTestBase : IDisposable
{
    protected LeaderboardApiTestBase()
    {
        this.Factory = new();
        this.Client  = CreateClient(this.Factory);
    }

    protected HttpClient Client { get; }

    protected LeaderboardApiFactory Factory { get; }

    public void Dispose()
    {
        this.Client.Dispose();
        this.Factory.Dispose();
        GC.SuppressFinalize(this);
    }

    protected static void AssertHasTraceId(
        ProblemDetails problem)
    {
        Assert.True(
            problem.Extensions.TryGetValue(
                "traceId",
                out var traceIdValue));

        var traceId = Assert.IsType<JsonElement>(
            traceIdValue);

        Assert.False(
            string.IsNullOrWhiteSpace(
                traceId.GetString()));
    }

    protected static HttpClient CreateClient(
        WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(
            new()
            {
                BaseAddress       = new("https://localhost"),
                AllowAutoRedirect = false,
            });
    }
}