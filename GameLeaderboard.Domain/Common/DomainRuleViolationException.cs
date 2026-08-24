namespace GameLeaderboard.Domain.Common;

public sealed class DomainRuleViolationException(
    string code,
    string message
)
    : Exception(message)
{
    public string Code { get; } = code;
}