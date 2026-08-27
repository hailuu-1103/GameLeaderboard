namespace GameLeaderboard.Application.Scores.SubmitScore;

using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Application.Common;
using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;
using GameLeaderboard.Domain.Scores;

public sealed class SubmitScoreHandler(
    ILeaderboardRepository leaderboardRepository,
    ISeasonRepository seasonRepository,
    IPlayerScoreRepository playerScoreRepository,
    IUnitOfWork unitOfWork,
    ScoreSubmissionPolicy submissionPolicy,
    TimeProvider timeProvider)
{
    public async Task<Result<SubmitScoreOutput>> HandleAsync(
        SubmitScoreCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var leaderboardId =
                LeaderboardId.Create(
                    command.LeaderboardId);

            var exists =
                await leaderboardRepository.ExistsAsync(
                    leaderboardId,
                    cancellationToken);

            if (!exists)
            {
                return Result<SubmitScoreOutput>.Failure(
                    new(
                        "leaderboard-not-found",
                        $"Leaderboard '{command.LeaderboardId}' does not exist.",
                        ApplicationErrorType.NotFound));
            }

            var now = timeProvider.GetUtcNow();

            var season =
                await seasonRepository.GetCurrentAsync(
                    leaderboardId,
                    now,
                    cancellationToken);

            if (season is null)
            {
                return Result<SubmitScoreOutput>.Failure(
                    new(
                        "season-not-found",
                        "No season is configured for score submission.",
                        ApplicationErrorType.Conflict));
            }

            var playerName =
                PlayerName.Create(command.PlayerName);

            var playerId =
                PlayerId.From(playerName);

            var submittedScore =
                Score.Create(command.Score);

            var playerScore =
                await playerScoreRepository.GetAsync(
                    season.Id,
                    playerId,
                    cancellationToken);

            long? previousHighScore;
            bool isNewHighScore;

            if (playerScore is null)
            {
                playerScore =
                    submissionPolicy.CreateFirstScore(
                        season,
                        playerName,
                        submittedScore,
                        now);

                playerScoreRepository.Add(playerScore);

                previousHighScore = null;
                isNewHighScore = true;
            }
            else
            {
                var decision =
                    submissionPolicy.Submit(
                        season,
                        playerScore,
                        submittedScore,
                        now);

                previousHighScore =
                    decision.PreviousBestScore.Value;

                isNewHighScore =
                    decision.IsNewHighScore;
            }

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Result<SubmitScoreOutput>.Success(
                new(
                    command.LeaderboardId,
                    playerScore.PlayerName.Value,
                    submittedScore.Value,
                    previousHighScore,
                    isNewHighScore));
        }
        catch (DomainRuleViolationException exception)
            when (IsExpectedClientError(exception.Code))
        {
            return Result<SubmitScoreOutput>.Failure(
                MapDomainError(exception));
        }
    }

    private static bool IsExpectedClientError(string code)
    {
        return code is
            "player-name-required"
            or "player-name-too-long"
            or "score-out-of-range"
            or "season-not-active";
    }

    private static ApplicationError MapDomainError(
        DomainRuleViolationException exception)
    {
        var type =
            exception.Code == "season-not-active"
                ? ApplicationErrorType.Conflict
                : ApplicationErrorType.Validation;

        return new(
            exception.Code,
            exception.Message,
            type);
    }
}