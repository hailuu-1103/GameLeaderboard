using GameLeaderboard.Domain.Common;

namespace GameLeaderboard.Domain.Scores;

public sealed record PlayerName
{
    public string Value { get; }

    private PlayerName(string value)
    {
        this.Value = value;
    }

    public static PlayerName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleViolationException(
                "player-name-required",
                "Player name must contain a non-whitespace character.");
        }

        var normalized = value.Trim();

        if (normalized.Length > 30)
        {
            throw new DomainRuleViolationException(
                "player-name-too-long",
                "Player name must not exceed 30 characters.");
        }

        return new(normalized);
    }

    public override string ToString()
    {
        return this.Value;
    }
}