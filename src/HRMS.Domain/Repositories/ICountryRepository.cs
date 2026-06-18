using HRMS.Domain.Entities;

namespace HRMS.Domain.Repositories;

/// <summary>
/// Repository contract for the Country aggregate root.
/// Implemented by Infrastructure; consumed by Application command/query handlers.
/// </summary>
public interface ICountryRepository
{
    Task<Country?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Country country, CancellationToken cancellationToken = default);

    void Update(Country country);

    void Delete(Country country);
}
