using Microsoft.AspNetCore.Mvc;

namespace GameLeaderboard.API.Controllers;

using System.Diagnostics;
using GameLeaderboard.API.Contracts.Leaderboards;
using GameLeaderboard.API.Contracts.Scores;
using GameLeaderboard.Application.Common;
using GameLeaderboard.Application.Leaderboards.GetPlayer;
using GameLeaderboard.Application.Leaderboards.GetTop;
using GameLeaderboard.Application.Scores.SubmitScore;

[ApiController]
[Route("api/leaderboards")]
public sealed class LeaderboardsController(SubmitScoreHandler submitScoreHandler, GetTopPlayerHandler getTopPlayerHandler, GetPlayerHandler getPlayerHandler) : ControllerBase
{
    [HttpGet("{leaderboardId}/top")]
    [ProducesResponseType<GetTopLeaderboardResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetTopLeaderboardResponse>> GetTopAsync(string leaderboardId, CancellationToken cancellationToken, [FromQuery] int limit = 10)
    {
        var result = await getTopPlayerHandler.HandleAsync(new(leaderboardId, limit), cancellationToken);

        if (result.IsSuccess)
        {
            var entries = result.Value.Entries
                .Select(entry => new LeaderboardEntryResponse(
                    entry.Rank,
                    entry.PlayerName,
                    entry.Score,
                    entry.AchievedAt))
                .ToArray();

            return this.Ok(new GetTopLeaderboardResponse(
                result.Value.LeaderboardId,
                result.Value.SeasonId,
                entries));
        }
        var error = result.Error;

        return error.Type switch
        {
            ApplicationErrorType.Validation =>
                this.Problem(
                    type: $"urn:game-leaderboard:errors:{error.Code}",
                    title: "Invalid leaderboard query.",
                    detail: error.Description,
                    statusCode: StatusCodes.Status400BadRequest),

            ApplicationErrorType.NotFound =>
                this.Problem(
                    type: $"urn:game-leaderboard:errors:{error.Code}",
                    title: error.Code == "season-not-found"
                        ? "Season not found."
                        : "Leaderboard not found.",
                    detail: error.Description,
                    statusCode: StatusCodes.Status404NotFound),

            _ => throw new UnreachableException(),
        };
    }

    [HttpGet("{leaderboardId}/player/{name}")]
    [ProducesResponseType<LeaderboardEntryResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeaderboardEntryResponse>> GetPlayerAsync(
        string leaderboardId,
        string name,
        CancellationToken cancellationToken
    )
    {
        var result = await getPlayerHandler.HandleAsync(new(leaderboardId, name), cancellationToken);

        if (result.IsSuccess)
        {
            return this.Ok(new LeaderboardEntryResponse(
                result.Value.Entry.Rank,
                result.Value.Entry.PlayerName,
                result.Value.Entry.Score,
                result.Value.Entry.AchievedAt));
        }
        var error = result.Error;
        if (error.Code.Equals("player-not-found"))
        {
            return this.Problem(
                type: $"urn:game-leaderboard:errors:{error.Code}",
                title: "Player not found.",
                detail: error.Description,
                statusCode: StatusCodes.Status404NotFound);
        }
        return error.Type switch
        {
            ApplicationErrorType.Validation =>
                this.Problem(
                    type: $"urn:game-leaderboard:errors:{error.Code}",
                    title: "Invalid leaderboard query.",
                    detail: error.Description,
                    statusCode: StatusCodes.Status400BadRequest),

            ApplicationErrorType.NotFound =>
                this.Problem(
                    type: $"urn:game-leaderboard:errors:{error.Code}",
                    title: error.Code == "season-not-found"
                        ? "Season not found."
                        : "Leaderboard not found.",
                    detail: error.Description,
                    statusCode: StatusCodes.Status404NotFound),

            _ => throw new UnreachableException(),
        };
    }

    [HttpPost("{leaderboardId}/scores")]
    [ProducesResponseType<SubmitScoreResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubmitScoreResponse>>
        SubmitScoreAsync(
            string leaderboardId,
            [FromBody] SubmitScoreRequest request,
            CancellationToken cancellationToken
        )
    {
        var result = await submitScoreHandler.HandleAsync(
            new(
                leaderboardId,
                request.PlayerName,
                request.Score),
            cancellationToken);

        if (result.IsSuccess)
        {
            var output = result.Value;

            return this.Ok(
                new SubmitScoreResponse(
                    output.LeaderboardId,
                    output.PlayerName,
                    output.SubmittedScore,
                    output.PreviousHighScore,
                    output.IsNewHighScore));
        }

        var error = result.Error;

        return error.Type switch
        {
            ApplicationErrorType.Validation =>
                this.Problem(
                    type:
                    $"urn:game-leaderboard:errors:{error.Code}",
                    title: "Invalid score submission.",
                    detail: error.Description,
                    statusCode:
                    StatusCodes.Status400BadRequest),

            ApplicationErrorType.NotFound =>
                this.Problem(
                    type:
                    $"urn:game-leaderboard:errors:{error.Code}",
                    title: "Leaderboard not found.",
                    detail: error.Description,
                    statusCode:
                    StatusCodes.Status404NotFound),

            ApplicationErrorType.Conflict =>
                this.Problem(
                    type:
                    $"urn:game-leaderboard:errors:{error.Code}",
                    title: "Score submission rejected.",
                    detail: error.Description,
                    statusCode:
                    StatusCodes.Status409Conflict),

            _ => throw new UnreachableException(),
        };
    }
}
