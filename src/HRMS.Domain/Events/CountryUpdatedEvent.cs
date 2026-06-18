using HRMS.Domain.Common;

namespace HRMS.Domain.Events;

/// <summary>
/// Raised when an existing Country aggregate is mutated via <c>Country.Update()</c>.
/// </summary>
public sealed record CountryUpdatedEvent(Guid CountryId, string Name, string CountryCode) : IDomainEvent;
