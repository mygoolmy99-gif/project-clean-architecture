using HRMS.Domain.Common;
using HRMS.Domain.Events;

namespace HRMS.Domain.Entities;

/// <summary>
/// Designation Aggregate Root — master data entity representing a job designation/position within a tenant scope.
/// All mutations go through explicit methods that enforce invariants and raise domain events.
/// 
/// STEP 1: Domain Entity Creation
/// - This file defines the core business entity for Designation
/// - Contains business logic and validation rules
/// - Inherits from BaseEntity<Guid> for common properties (Id, TenantId, CreatedAt, etc.)
/// - Implements IAggregateRoot to mark it as an aggregate root in DDD
/// </summary>
public sealed class Designation : BaseEntity<Guid>, IAggregateRoot
{
    // Designation name - required field, cannot be empty
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// EF Core materialization constructor (private - used only by EF Core to hydrate entities from database)
    /// STEP 2: Private Constructor for EF Core
    /// - EF Core uses this to create instances when reading from database
    /// - Made private to prevent direct instantiation outside the domain
    /// </summary>
    private Designation()
    {
    }

    /// <summary>
    /// Private constructor used by factory method
    /// STEP 3: Domain Constructor
    /// - Called only by Create() factory method
    /// - Initializes the entity with valid state
    /// - Sets IsActive to true by default for new designations
    /// </summary>
    private Designation(Guid id, Guid tenantId, string name)
        : base(id, tenantId)
    {
        Name = name;
        IsActive = true;
    }

    /// <summary>
    /// Factory method — the only way to create a Designation aggregate.
    /// STEP 4: Factory Method for Creating New Designations
    /// - Validates business rules before creation
    /// - Raises Domain Event for audit/logging purposes
    /// - Returns a new Designation instance with valid state
    /// 
    /// Usage: var designation = Designation.Create(tenantId, "Software Engineer");
    /// </summary>
    /// <param name="tenantId">The tenant ID (for multi-tenancy support)</param>
    /// <param name="name">The designation name (required, cannot be empty)</param>
    /// <returns>New Designation instance</returns>
    /// <exception cref="DomainException">Thrown when validation fails</exception>
    public static Designation Create(Guid tenantId, string name)
    {
        // Business Rule: Name cannot be empty or whitespace
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Designation name cannot be empty.");

        // Create new designation with trimmed name
        var designation = new Designation(
            Guid.NewGuid(),
            tenantId,
            name.Trim());

        // Raise domain event for tracking creation (useful for audit logs, notifications, etc.)
        designation.AddDomainEvent(new DesignationCreatedEvent(designation.Id, designation.Name));

        return designation;
    }

    /// <summary>
    /// Mutates the aggregate state. Validates invariants and raises DesignationUpdatedEvent.
    /// STEP 5: Update Method for Modifying Designations
    /// - Enforces business rules before allowing updates
    /// - Updates the UpdatedAt timestamp automatically
    /// - Raises domain event for change tracking
    /// 
    /// Usage: designation.Update("Senior Software Engineer");
    /// </summary>
    /// <param name="name">New designation name</param>
    /// <exception cref="DomainException">Thrown when validation fails</exception>
    public void Update(string name)
    {
        // Business Rule: Name cannot be empty or whitespace
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Designation name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event for tracking updates
        AddDomainEvent(new DesignationUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Activates the designation (soft delete reversal)
    /// STEP 6: Activate Method
    /// - Sets IsActive flag to true
    /// - Updates the UpdatedAt timestamp
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the designation (soft delete)
    /// STEP 7: Deactivate Method
    /// - Sets IsActive flag to false (soft delete pattern)
    /// - Updates the UpdatedAt timestamp
    /// - Preferred over hard delete for audit purposes
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
