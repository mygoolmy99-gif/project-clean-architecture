using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.CreateCountry;

public sealed class CreateCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService) : IRequestHandler<CreateCountryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.GetCurrentTenantId();
        var country = Country.Create(tenantId, request.Name, request.CountryCode, request.PhoneCode);
        
        await countryRepository.AddAsync(country, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return country.Id;
    }
}
