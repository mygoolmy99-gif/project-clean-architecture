namespace HRMS.Domain.Common;

/// <summary>
/// Marker interface identifying an entity as an Aggregate Root.
/// Only aggregate roots should have repositories and be directly persisted.
/// </summary>
public interface IAggregateRoot;
