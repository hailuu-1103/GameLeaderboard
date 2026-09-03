namespace GameLeaderboard.Infrastructure.Persistence.InMemory;

using GameLeaderboard.Application.Abstractions.Persistence;

internal class InMemoryUnitOfWork : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}