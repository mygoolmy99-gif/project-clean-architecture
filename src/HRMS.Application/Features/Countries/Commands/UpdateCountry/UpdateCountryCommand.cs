using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.UpdateCountry;

/// <summary>
/// Command to update an existing Country aggregate. Includes ConcurrencyStamp for optimistic concurrency.
/// </summary>
public sealed record UpdateCountryCommand(
    Guid Id,
    string Name,
    string CountryCode,
    string PhoneCode,
    DateTime? ConcurrencyStamp) : IRequest<Result<Guid>>;
