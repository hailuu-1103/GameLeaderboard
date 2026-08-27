namespace GameLeaderboard.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(
        CancellationToken cancellationToken);
}