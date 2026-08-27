namespace GameLeaderboard.Application.Common;

public sealed record ApplicationError(
    string               Code,
    string               Description,
    ApplicationErrorType Type
);