using Microsoft.AspNetCore.Mvc;

namespace GameLeaderboard.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using GameLeaderboard.Api.Services;

[ApiController]
[Route("api/leaderboards")]
public sealed class LeaderboardsController(ILeaderboardService leaderboardService) : ControllerBase
{
    [HttpGet("{leaderboardId}/top")]
    public ActionResult<TopLeaderboardResponse> GetTop(
        string          leaderboardId,
        [FromQuery] int limit = 10)
    {
        if (limit is < 1 or > 100)
        {
            return this.BadRequest(new
            {
                Message = "Limit must be between 1 and 100.",
            });
        }

        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.NotFound(new
            {
                Message = "Leaderboard not found.",
            });
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

    [HttpGet("{leaderboardId}/players/{name}")]
    public ActionResult<PlayerResponse> GetPlayer(
        string leaderboardId,
        string name)
    {
        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.NotFound(new
            {
                Message = "Leaderboard not found.",
            });
        }

        var player = leaderboardService.GetPlayer(
            leaderboardId,
            name.Trim());

        if (player is null)
        {
            return this.NotFound(new
            {
                Message = "Player not found.",
            });
        }

        var response = new PlayerResponse(
            player.Rank,
            player.PlayerName,
            player.Score);

        return this.Ok(response);
    }

    [HttpPost("{leaderboardId}/scores")]
    public ActionResult<SubmitScoreResponse> SubmitScore(
        string                        leaderboardId,
        [FromBody] SubmitScoreRequest request
    )
    {
        if (!leaderboardService.LeaderboardExists(leaderboardId))
        {
            return this.NotFound(new
            {
                Message = "Leaderboard not found.",
            });
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
        MinimumLength = 1,
        ErrorMessage = "Player name must contain between 1 and 30 characters.")]
    string PlayerName,
    [Range(
        0,
        1_000_000_000,
        ErrorMessage = "Score must be between 0 and 1,000,000,000.")]
    long Score
);

public sealed record SubmitScoreResponse(
    string LeaderboardId,
    string PlayerName,
    long   SubmittedScore,
    long?  PreviousHighScore,
    bool   IsNewHighScore
);