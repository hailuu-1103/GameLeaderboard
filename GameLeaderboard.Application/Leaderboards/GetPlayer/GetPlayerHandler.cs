namespace GameLeaderboard.Application.Leaderboards.GetPlayer;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Application.Common;
using GameLeaderboard.Application.Leaderboards.GetTop;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;

public class GetPlayerHandler(
    ILeaderboardRepository leaderboardRepository,
    ISeasonRepository      seasonRepository,
    ILeaderboardQueries    leaderboardQueries,
    TimeProvider           timeProvider
)
{
    public async Task<Result<GetPlayerOutput>> HandleAsync(
        GetPlayerQuery    query,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var leaderboardId = LeaderboardId.Create(query.LeaderboardId);
            var exists =
                await leaderboardRepository.ExistsAsync(
                    leaderboardId,
                    cancellationToken);
            if (!exists)
            {
                return Result<GetPlayerOutput>.Failure(
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
                return Result<GetPlayerOutput>.Failure(
                    new(
                        "season-not-found",
                        $"No active season exists for leaderboard '{leaderboardId.Value}'.",
                        ApplicationErrorType.NotFound));
            }

            var player =
                await leaderboardQueries.GetPlayerAsync(
                    leaderboardId.Value,
                    query.Name,
                    cancellationToken);
            if (player is null)
                return Result<GetPlayerOutput>.Failure(
                    new("player-not-found",
                        $"Player '{query.Name}' does not exist in leaderboard '{leaderboardId}'.",
                        ApplicationErrorType.NotFound));

            return Result<GetPlayerOutput>.Success(
                new(
                    new(player.Rank, player.PlayerName, player.Score, player.AchievedAt)));
        }
        catch (DomainRuleViolationException exception) when (IsExpectedClientError(exception.Code))
        {
            return Result<GetPlayerOutput>.Failure(
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