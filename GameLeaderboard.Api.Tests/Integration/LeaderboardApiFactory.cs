using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameLeaderboard.API.Tests.Integration;

public sealed class LeaderboardApiFactory
    : WebApplicationFactory<Program>
{
    public static DateTimeOffset UtcNow { get; } =
        new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(
                new FixedTimeProvider(UtcNow));
        });
    }
}
