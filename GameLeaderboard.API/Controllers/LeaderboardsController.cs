using Microsoft.AspNetCore.Mvc;

namespace GameLeaderboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LeaderboardsController : ControllerBase
{
    [HttpGet("{leaderboardId}/top")]
    public ActionResult GetTop(string leaderboardId, [FromQuery] int limit = 10)
    {
        return Ok(new
        {
            LeaderboardId = leaderboardId,
            Entries = new[]
            {
                new { Rank = 1, PlayerName = "Hai", Score   = 15000 },
                new { Rank = 2, PlayerName = "Alice", Score = 12000 }
            }
        });
    }
}