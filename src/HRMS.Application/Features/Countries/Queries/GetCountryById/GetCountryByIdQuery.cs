using HRMS.Application.Common;
using HRMS.Application.DTOs;
using MediatR;

namespace HRMS.Application.Features.Countries.Queries.GetCountryById;

/// <summary>
/// Query to retrieve a single Country by its unique identifier.
/// </summary>
public sealed record GetCountryByIdQuery(Guid Id) : IRequest<Result<CountryDto>>;
