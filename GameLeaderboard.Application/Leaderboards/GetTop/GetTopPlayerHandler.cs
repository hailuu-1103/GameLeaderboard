namespace GameLeaderboard.Application.Leaderboards.GetTop;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Application.Common;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;

public sealed class GetTopPlayerHandler(
    ILeaderboardRepository leaderboardRepository,
    ISeasonRepository      seasonRepository,
    ILeaderboardQueries    leaderboardQueries,
    TimeProvider           timeProvider
)
{
    public async Task<Result<GetTopPlayerOutput>> HandleAsync(
        GetTopPlayerQuery query,
        CancellationToken cancellationToken
    )
    {
        try
        {
            if (query.Limit is < 1 or > 100)
            {
                return Result<GetTopPlayerOutput>.Failure(
                    new(
                        "limit-out-of-range",
                        "Limit must be between 1 and 100.",
                        ApplicationErrorType.Validation));
            }

            var leaderboardId = LeaderboardId.Create(query.LeaderboardId);
            var exists =
                await leaderboardRepository.ExistsAsync(
                    leaderboardId,
                    cancellationToken);
            if (!exists)
            {
                return Result<GetTopPlayerOutput>.Failure(
                    new(
                        "leaderboard-not-found",
                        $"Leaderboard '{query.LeaderboardId}' does not exist.",
                        ApplicationErrorType.NotFound));
            }

            var season =
                await seasonRepository.GetCurrentAsync(
                    leaderboardId,
                    timeProvider.GetUtcNow(),
                    cancellationToken);

            if (season is null)
            {
                return Result<GetTopPlayerOutput>.Failure(
                    new(
                        "season-not-found",
                        $"No active season exists for leaderboard '{leaderboardId.Value}'.",
                        ApplicationErrorType.NotFound));
            }

            var readModels =
                await leaderboardQueries.GetTopBySeasonAsync(
                    season.Id,
                    query.Limit,
                    cancellationToken);

            var entries = readModels
                .Select(entry => new LeaderboardEntryOutput(
                    entry.Rank,
                    entry.PlayerName,
                    entry.Score,
                    entry.AchievedAt))
                .ToArray();

            return Result<GetTopPlayerOutput>.Success(
                new(
                    leaderboardId.Value,
                    season.Id.Value,
                    entries));
        }
        catch (DomainRuleViolationException exception) when (IsExpectedClientError(exception.Code))
        {
            return Result<GetTopPlayerOutput>.Failure(
                new(
                    exception.Code,
                    exception.Message,
                    ApplicationErrorType.Validation));
        }
    }

    private static bool IsExpectedClientError(string code)
    {
        return code is "leaderboard-id-required";
    }
}