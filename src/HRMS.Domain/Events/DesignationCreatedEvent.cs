using HRMS.Domain.Common;

namespace HRMS.Domain.Events;

/// <summary>
/// STEP 8: Domain Event for Designation Creation
/// Raised when a new Designation aggregate is created via Designation.Create()
/// 
/// Purpose:
/// - Tracks when designations are created in the system
/// - Can trigger notifications, audit logs, or other side effects
/// - Follows the domain event pattern from DDD
/// </summary>
public sealed record DesignationCreatedEvent(Guid DesignationId, string Name) : IDomainEvent;
