namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

public sealed class InMemoryLeaderboardStore
{
    internal Lock SyncRoot { get; } = new();

    internal Dictionary<string, Leaderboard> Leaderboards { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<string, Season> Seasons { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    internal Dictionary<
        (SeasonId SeasonId, PlayerId PlayerId),
        PlayerScore> PlayerScores { get; } = new();

    public InMemoryLeaderboardStore()
    {
        var leaderboardId = LeaderboardId.Create("classic");
        var seasonId      = SeasonId.Create("season-1");
        var season        = Season.Create(seasonId, leaderboardId, DateTime.Parse("2026-08-21T10:00:00"), DateTime.Parse("2026-09-25T09:00:00"));
        season.Activate(DateTime.Parse("2026-08-22T09:00:00"));
        var playerScore1 = PlayerScore.Create(seasonId, PlayerName.Create("Hai"), Score.Create(15_000), DateTime.Parse("2026-08-21T10:00:00"));
        var playerScore2 = PlayerScore.Create(seasonId, PlayerName.Create("Alice"), Score.Create(12_000), DateTime.Parse("2026-08-21T11:00:00"));
        var playerScore3 = PlayerScore.Create(seasonId, PlayerName.Create("Bob"), Score.Create(20_000), DateTime.Parse("2026-08-21T12:00:00"));

        this.PlayerScores.Add(
            (seasonId, playerScore1.PlayerId),
            playerScore1);
        this.PlayerScores.Add(
            (seasonId, playerScore2.PlayerId),
            playerScore2);
        this.PlayerScores.Add(
            (seasonId, playerScore3.PlayerId),
            playerScore3);

        this.Leaderboards.Add(
            leaderboardId.Value,
            Leaderboard.Create(leaderboardId));

        this.Seasons.Add(seasonId.Value, season);
    }
}
