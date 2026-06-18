namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Abstracts the transactional boundary. SaveChangesAsync commits all tracked changes.
/// Implemented by Infrastructure (wraps DbContext.SaveChangesAsync).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
