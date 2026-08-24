using GameLeaderboard.Domain.Common;
using GameLeaderboard.Domain.Leaderboard;

namespace GameLeaderboard.Domain.Season;

public sealed class Season
{
    public SeasonId Id { get; }
    public LeaderboardId LeaderboardId { get; }
    public DateTimeOffset StartsAt { get; }
    public DateTimeOffset EndsAt { get; }
    public SeasonStatus Status { get; private set; }

    private Season(SeasonId id, LeaderboardId leaderboardId, DateTimeOffset startsAt, DateTimeOffset endsAt, SeasonStatus status)
    {
        this.Id = id;
        this.LeaderboardId = leaderboardId;
        this.StartsAt = startsAt;
        this.EndsAt = endsAt;
        this.Status = status;
    }

    public static Season Create(SeasonId id, LeaderboardId leaderboardId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        // Validate schedule.
        if (startsAt >= endsAt)
        {
            throw new DomainRuleViolationException("season-invalid-time", "Start time must be smaller than end time");
        }

        return new Season(id, leaderboardId, startsAt, endsAt, SeasonStatus.Scheduled);
    }

    public void Activate(DateTimeOffset now)
    {
        if (this.Status == SeasonStatus.Closed)
        {
            throw new DomainRuleViolationException("closed-season-reopening-rule",
                "Closed season can not be reopened.");
        }
        if (this.Status == SeasonStatus.Active)
        {
            throw new DomainRuleViolationException("active-season-reopening-rule",
                "Active season can not be reactivated.");
        }

        if (now < this.StartsAt || now >= this.EndsAt) return;
        this.Status = SeasonStatus.Active;
    }

    public bool AcceptsScoresAt(DateTimeOffset now)
    {
        return this.Status == SeasonStatus.Active
            && now >= this.StartsAt
            && now < this.EndsAt;
    }

    public void Close(DateTimeOffset now)
    {
        // Protect Active → Closed transition.
        if (this.Status != SeasonStatus.Active)
        {
            throw new DomainRuleViolationException("season-not-active",
                "Only an active season can be closed.");
        }
        this.Status = SeasonStatus.Closed;
    }
}