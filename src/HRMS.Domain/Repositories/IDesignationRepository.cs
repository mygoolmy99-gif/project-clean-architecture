using HRMS.Domain.Entities;

namespace HRMS.Domain.Repositories;

/// <summary>
/// STEP 10: Repository Interface for Designation
/// Contract for the Designation aggregate root.
/// Implemented by Infrastructure; consumed by Application command/query handlers.
/// 
/// Purpose:
/// - Defines the operations available for Designation data access
/// - Abstracts the database implementation from the domain layer
/// - Follows the repository pattern from DDD
/// </summary>
public interface IDesignationRepository
{
    /// <summary>
    /// Retrieves a designation by its unique identifier
    /// </summary>
    Task<Designation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all designations for the current tenant
    /// </summary>
    Task<IReadOnlyList<Designation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new designation to the database
    /// </summary>
    Task AddAsync(Designation designation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing designation for update
    /// </summary>
    void Update(Designation designation);

    /// <summary>
    /// Marks a designation for deletion (soft delete via Deactivate method)
    /// </summary>
    void Delete(Designation designation);
}
