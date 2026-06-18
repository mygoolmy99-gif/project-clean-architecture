using HRMS.Application.Common;
using HRMS.Application.DTOs;
using MediatR;

namespace HRMS.Application.Features.Countries.Queries.GetAllCountries;

/// <summary>
/// Query to retrieve all Countries within the current tenant scope.
/// </summary>
public sealed record GetAllCountriesQuery : IRequest<Result<IReadOnlyList<CountryDto>>>;
