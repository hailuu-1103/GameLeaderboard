namespace GameLeaderboard.Domain.Scores;

public sealed record PlayerId
{
    public string Value { get; }

    private PlayerId(string value)
    {
        this.Value = value;
    }

    public static PlayerId From(PlayerName playerName)
    {
        return new PlayerId(
            playerName.Value.ToUpperInvariant());
    }

    public override string ToString()
    {
        return this.Value;
    }
}