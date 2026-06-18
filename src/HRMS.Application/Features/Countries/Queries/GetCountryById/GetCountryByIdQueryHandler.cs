using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Domain.Repositories;
using Mapster;
using MediatR;

namespace HRMS.Application.Features.Countries.Queries.GetCountryById;

public sealed class GetCountryByIdQueryHandler(
    ICountryRepository countryRepository) : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (country is null)
        {
            return Result<CountryDto>.NotFound($"Country with ID '{request.Id}' was not found.");
        }

        var dto = country.Adapt<CountryDto>();
        return Result<CountryDto>.Success(dto);
    }
}
