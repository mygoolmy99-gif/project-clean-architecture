using HRMS.Domain.Common;

namespace HRMS.Domain.Events;

/// <summary>
/// Raised when a new Country aggregate is created via <c>Country.Create()</c>.
/// </summary>
public sealed record CountryCreatedEvent(Guid CountryId, string Name, string CountryCode) : IDomainEvent;
