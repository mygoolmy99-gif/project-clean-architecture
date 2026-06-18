using HRMS.Application.DTOs;
using HRMS.Domain.Entities;
using Mapster;

namespace HRMS.Application.Mapping;

/// <summary>
/// Mapster type adapter configuration for Country -> CountryDto.
/// </summary>
public sealed class CountryMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Country, CountryDto>()
            .Map(dest => dest.CountryCode, src => src.CountryCode.Value);
    }
}
