using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Repositories;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.UpdateCountry;

public sealed class UpdateCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCountryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (country is null)
        {
            return Result<Guid>.NotFound($"Country with ID '{request.Id}' was not found.");
        }

        if (request.ConcurrencyStamp.HasValue && request.ConcurrencyStamp != country.UpdatedAt)
        {
            return Result<Guid>.Failure(new Error("CONCURRENCY_CONFLICT", "The country has been modified by another user."));
        }

        country.Update(request.Name, request.CountryCode, request.PhoneCode);
        
        countryRepository.Update(country);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return country.Id;
    }
}
