using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Domain.Repositories;
using Mapster;
using MediatR;

namespace HRMS.Application.Features.Countries.Queries.GetAllCountries;

public sealed class GetAllCountriesQueryHandler(
    ICountryRepository countryRepository) : IRequestHandler<GetAllCountriesQuery, Result<IReadOnlyList<CountryDto>>>
{
    public async Task<Result<IReadOnlyList<CountryDto>>> Handle(GetAllCountriesQuery request, CancellationToken cancellationToken)
    {
        var countries = await countryRepository.GetAllAsync(cancellationToken);
        
        var dtos = countries.Adapt<List<CountryDto>>();
        
        return Result<IReadOnlyList<CountryDto>>.Success(dtos);
    }
}
