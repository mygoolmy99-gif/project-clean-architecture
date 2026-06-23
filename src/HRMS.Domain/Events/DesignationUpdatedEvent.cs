using HRMS.Domain.Common;

namespace HRMS.Domain.Events;

/// <summary>
/// STEP 9: Domain Event for Designation Update
/// Raised when a Designation aggregate is updated via Designation.Update()
/// 
/// Purpose:
/// - Tracks when designations are modified in the system
/// - Can trigger notifications, audit logs, or other side effects
/// - Follows the domain event pattern from DDD
/// </summary>
public sealed record DesignationUpdatedEvent(Guid DesignationId, string Name) : IDomainEvent;
