namespace GameLeaderboard.Domain.Season;

using GameLeaderboard.Domain.Common;

public sealed record SeasonId
{
    public string Value { get; }

    private SeasonId(string value)
    {
        this.Value = value;
    }

    public static SeasonId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleViolationException(
                "season-id-required",
                "Season id must not null.");
        }

        var normalized = value.Trim();
        return new(normalized);
    }
}