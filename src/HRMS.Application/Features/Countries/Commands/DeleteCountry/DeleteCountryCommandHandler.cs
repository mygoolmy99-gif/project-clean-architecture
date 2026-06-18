using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Repositories;
using MediatR;

namespace HRMS.Application.Features.Countries.Commands.DeleteCountry;

public sealed class DeleteCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCountryCommand, Result>
{
    public async Task<Result> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await countryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (country is null)
        {
            return Result.NotFound($"Country with ID '{request.Id}' was not found.");
        }

        countryRepository.Delete(country);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
