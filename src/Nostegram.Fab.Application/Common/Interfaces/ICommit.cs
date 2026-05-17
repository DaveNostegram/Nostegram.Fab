namespace Nostegram.Fab.Application.Common.Interfaces;

public interface ICommit
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
