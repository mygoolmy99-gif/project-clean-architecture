using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.DeleteCountry;

/// <summary>
/// Command to permanently delete a Country aggregate.
/// </summary>
public sealed record DeleteCountryCommand(Guid Id) : IRequest<Result>;
