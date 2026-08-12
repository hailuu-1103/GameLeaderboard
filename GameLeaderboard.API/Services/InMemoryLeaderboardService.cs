namespace GameLeaderboard.Api.Services;

public sealed class InMemoryLeaderboardService : ILeaderboardService
{
    private readonly Lock leaderboardLock = new();

    private readonly Dictionary<string, Dictionary<string, long>>
        leaderboards =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["classic"] =
                    new(
                        StringComparer.OrdinalIgnoreCase)
                    {
                        ["Hai"] = 15_000,
                        ["Alice"] = 12_000,
                        ["Bob"] = 9_500,
                    },
            };

    public bool LeaderboardExists(string leaderboardId)
    {
        lock (this.leaderboardLock)
        {
            return this.leaderboards.ContainsKey(leaderboardId);
        }
    }

    public IReadOnlyList<LeaderboardEntry> GetTop(
        string leaderboardId,
        int limit)
    {
        lock (this.leaderboardLock)
        {
            if (!this.leaderboards.TryGetValue(
                    leaderboardId,
                    out var players))
            {
                return [];
            }

            return players
                .OrderByDescending(player => player.Value)
                .ThenBy(
                    player => player.Key,
                    StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .Select((player, index) =>
                    new LeaderboardEntry(
                        index + 1,
                        player.Key,
                        player.Value))
                .ToArray();
        }
    }

    public LeaderboardEntry? GetPlayer(
        string leaderboardId,
        string playerName)
    {
        lock (this.leaderboardLock)
        {
            if (!this.leaderboards.TryGetValue(
                    leaderboardId,
                    out var players))
            {
                return null;
            }

            var ranking = players
                .OrderByDescending(player => player.Value)
                .ThenBy(
                    player => player.Key,
                    StringComparer.OrdinalIgnoreCase)
                .Select((player, index) =>
                    new LeaderboardEntry(
                        index + 1,
                        player.Key,
                        player.Value));

            return ranking.FirstOrDefault(entry =>
                string.Equals(
                    entry.PlayerName,
                    playerName,
                    StringComparison.OrdinalIgnoreCase));
        }
    }

    public SubmitScoreResult SubmitScore(
        string leaderboardId,
        string playerName,
        long score)
    {
        lock (this.leaderboardLock)
        {
            var players = this.leaderboards[leaderboardId];

            var playerExists = players.TryGetValue(
                playerName,
                out var previousHighScore);

            var isNewHighScore =
                !playerExists ||
                score > previousHighScore;

            if (isNewHighScore)
            {
                players[playerName] = score;
            }

            return new SubmitScoreResult(
                playerExists ? previousHighScore : null,
                isNewHighScore);
        }
    }
}