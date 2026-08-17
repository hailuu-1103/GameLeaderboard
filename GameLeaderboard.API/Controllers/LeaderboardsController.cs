using Microsoft.AspNetCore.Mvc;

namespace GameLeaderboard.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using GameLeaderboard.Api.Services;

[ApiController]
[Route("api/leaderboards")]
public sealed class LeaderboardsController(ILeaderboardService leaderboardService) : ControllerBase
{
    [HttpGet("{leaderboardId}/top")]
    [ProducesResponseType<TopLeaderboardResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public ActionResult<TopLeaderboardResponse> GetTop(
        string leaderboardId,
        [FromQuery]
        [Range(
            1,
            100,
            ErrorMessage = "Limit must be between 1 and 100.")]
        int limit = 10)
    {
        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.Problem(
                type:
                "urn:game-leaderboard:errors:leaderboard-not-found",
                title: "Leaderboard not found.",
                detail:
                $"Leaderboard '{leaderboardId}' does not exist.",
                statusCode: StatusCodes.Status404NotFound,
                instance: this.Request.Path);
        }

        var entries = leaderboardService
            .GetTop(leaderboardId, limit)
            .Select(entry => new LeaderboardEntryResponse(
                entry.Rank,
                entry.PlayerName,
                entry.Score))
            .ToArray();

        var response = new TopLeaderboardResponse(
            leaderboardId,
            entries);

        return this.Ok(response);
    }

    [HttpGet("{leaderboardId}/player/{name}")]
    [ProducesResponseType<PlayerResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public ActionResult<PlayerResponse> GetPlayer(
        string leaderboardId,
        string name)
    {
        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.Problem(
                type:
                "urn:game-leaderboard:errors:leaderboard-not-found",
                title: "Leaderboard not found.",
                detail:
                $"Leaderboard '{leaderboardId}' does not exist.",
                statusCode: StatusCodes.Status404NotFound,
                instance: this.Request.Path);
        }

        var player = leaderboardService.GetPlayer(
            leaderboardId,
            name.Trim());

        if (player is null)
        {
            return this.Problem(
                type:
                "urn:game-leaderboard:errors:player-not-found",
                title: "Player not found.",
                detail:
                $"Player '{name}' does not exist in leaderboard '{leaderboardId}'.",
                statusCode: StatusCodes.Status404NotFound,
                instance: this.Request.Path);
        }

        var response = new PlayerResponse(
            player.Rank,
            player.PlayerName,
            player.Score);

        return this.Ok(response);
    }

    [HttpPost("{leaderboardId}/scores")]
    [ProducesResponseType<SubmitScoreResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public ActionResult<SubmitScoreResponse> SubmitScore(
        string                        leaderboardId,
        [FromBody] SubmitScoreRequest request
    )
    {
        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.Problem(
                type:
                "urn:game-leaderboard:errors:leaderboard-not-found",
                title: "Leaderboard not found.",
                detail:
                $"Leaderboard '{leaderboardId}' does not exist.",
                statusCode: StatusCodes.Status404NotFound,
                instance: this.Request.Path);
        }
        var playerName = request.PlayerName.Trim();
        var result     = leaderboardService.SubmitScore(leaderboardId, playerName, request.Score);
        var response = new SubmitScoreResponse(
            leaderboardId,
            playerName,
            request.Score,
            result.PreviousHighScore,
            result.IsNewHighScore);

        return this.Ok(response);
    }
}

public sealed record PlayerResponse(
    int    Rank,
    string PlayerName,
    long   Score
);

public sealed record LeaderboardEntryResponse(
    int    Rank,
    string PlayerName,
    long   Score
);

public sealed record TopLeaderboardResponse(
    string                                  LeaderboardId,
    IReadOnlyList<LeaderboardEntryResponse> Entries
);

public sealed record SubmitScoreRequest(
    [Required(ErrorMessage = "Player name is required.")]
    [StringLength(
        30,
        ErrorMessage =
            "Player name must not exceed 30 characters.")]
    [RegularExpression(
        @".*\S.*",
        ErrorMessage =
            "Player name must contain at least one non-whitespace character.")]
    string PlayerName,

    [Range(
        0,
        1_000_000_000,
        ErrorMessage =
            "Score must be between 0 and 1,000,000,000.")]
    long Score);

public sealed record SubmitScoreResponse(
    string LeaderboardId,
    string PlayerName,
    long   SubmittedScore,
    long?  PreviousHighScore,
    bool   IsNewHighScore
);