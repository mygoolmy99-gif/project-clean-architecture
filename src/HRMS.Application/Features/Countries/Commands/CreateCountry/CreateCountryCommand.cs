using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.CreateCountry;

/// <summary>
/// Command to create a new Country aggregate within the current tenant scope.
/// </summary>
public sealed record CreateCountryCommand(
    string Name,
    string CountryCode,
    string PhoneCode) : IRequest<Result<Guid>>;
