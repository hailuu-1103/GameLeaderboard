namespace GameLeaderboard.API.Tests.UnitTests.Scores.SubmitScore;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Application.Scores.SubmitScore;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Domain.Season;

internal sealed class SubmitScoreHandlerFixture
{
    internal static readonly DateTimeOffset UTC_NOW =
        new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);

    internal FakeLeaderboardRepository LeaderboardRepository { get; } = new();
    internal FakeSeasonRepository SeasonRepository { get; } = new();
    internal FakePlayerScoreRepository PlayerScoreRepository { get; } = new();
    internal SpyUnitOfWork UnitOfWork { get; } = new();
    internal FixedTimeProvider TimeProvider { get; } = new(UTC_NOW);

    internal SubmitScoreHandlerFixture()
    {
        this.SeasonRepository.Season = CreateActiveSeason();
    }

    internal SubmitScoreHandler CreateHandler()
    {
        return new(
            this.LeaderboardRepository,
            this.SeasonRepository,
            this.PlayerScoreRepository,
            this.UnitOfWork,
            new(),
            this.TimeProvider);
    }

    internal static SubmitScoreCommand CreateCommand(
        string playerName = "David",
        long score = 13_000)
    {
        return new("classic", playerName, score);
    }

    internal static Season CreateActiveSeason()
    {
        var season = CreateScheduledSeason();
        season.Activate(UTC_NOW);
        return season;
    }

    internal static Season CreateScheduledSeason()
    {
        return Season.Create(
            SeasonId.Create("season-1"),
            LeaderboardId.Create("classic"),
            UTC_NOW.AddDays(-1),
            UTC_NOW.AddDays(1));
    }

    internal static PlayerScore CreatePlayerScore(
        SeasonId seasonId,
        long bestScore,
        DateTimeOffset achievedAt)
    {
        return PlayerScore.Create(
            seasonId,
            PlayerName.Create("Alice"),
            Score.Create(bestScore),
            achievedAt);
    }
}

internal sealed class FakeLeaderboardRepository : ILeaderboardRepository
{
    internal bool Exists { get; set; } = true;
    internal int CallCount { get; private set; }
    internal LeaderboardId? ReceivedLeaderboardId { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<bool> ExistsAsync(
        LeaderboardId leaderboardId,
        CancellationToken cancellationToken)
    {
        this.CallCount++;
        this.ReceivedLeaderboardId = leaderboardId;
        this.ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(this.Exists);
    }
}

internal sealed class FakeSeasonRepository : ISeasonRepository
{
    internal Season? Season { get; set; }
    internal int CallCount { get; private set; }
    internal LeaderboardId? ReceivedLeaderboardId { get; private set; }
    internal DateTimeOffset ReceivedNow { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<Season?> GetCurrentAsync(
        LeaderboardId leaderboardId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        this.CallCount++;
        this.ReceivedLeaderboardId = leaderboardId;
        this.ReceivedNow = now;
        this.ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(this.Season);
    }
}

internal sealed class FakePlayerScoreRepository : IPlayerScoreRepository
{
    internal PlayerScore? PlayerScore { get; set; }
    internal PlayerScore? AddedPlayerScore { get; private set; }
    internal int GetCallCount { get; private set; }
    internal int AddCallCount { get; private set; }
    internal SeasonId? ReceivedSeasonId { get; private set; }
    internal PlayerId? ReceivedPlayerId { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<PlayerScore?> GetAsync(
        SeasonId seasonId,
        PlayerId playerId,
        CancellationToken cancellationToken)
    {
        this.GetCallCount++;
        this.ReceivedSeasonId = seasonId;
        this.ReceivedPlayerId = playerId;
        this.ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(this.PlayerScore);
    }

    public void Add(PlayerScore playerScore)
    {
        this.AddCallCount++;
        this.AddedPlayerScore = playerScore;
    }
}

internal sealed class SpyUnitOfWork : IUnitOfWork
{
    internal int SaveCallCount { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        this.SaveCallCount++;
        this.ReceivedCancellationToken = cancellationToken;
        return Task.CompletedTask;
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
    {
        return utcNow;
    }
}