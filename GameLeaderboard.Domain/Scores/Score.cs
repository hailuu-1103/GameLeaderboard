using GameLeaderboard.Domain.Common;

namespace GameLeaderboard.Domain.Scores;

public readonly record struct Score
{
    public const long MINIMUM = 0;
    public const long MAXIMUM = 1_000_000_000;

    public long Value { get; }

    private Score(long value)
    {
        this.Value = value;
    }

    public static Score Create(long value)
    {
        if (value is < MINIMUM or > MAXIMUM)
        {
            throw new DomainRuleViolationException(
                "score-out-of-range",
                $"Score must be between {MINIMUM} and {MAXIMUM}.");
        }

        return new(value);
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }
}