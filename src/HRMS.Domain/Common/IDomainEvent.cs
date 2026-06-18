namespace HRMS.Domain.Common;

/// <summary>
/// Marker interface for domain events. Intentionally has no dependency on MediatR
/// or any external library — the Domain layer remains pure.
/// Infrastructure wires these to MediatR INotification at the boundary.
/// </summary>
public interface IDomainEvent;
