namespace GameLeaderboard.Domain.Leaderboard;

using GameLeaderboard.Domain.Common;

public sealed record LeaderboardId
{
    public string Value { get; }

    private LeaderboardId(string value)
    {
        this.Value = value;
    }

    public static LeaderboardId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleViolationException(
                "leaderboard-id-required",
                "Leaderboard-ID required");
        }

        var normalized = value.Trim();
        return new(normalized);
    }

    public override string ToString()
    {
        return this.Value;
    }
}