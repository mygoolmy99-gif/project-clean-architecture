using FluentValidation;

namespace HRMS.Application.Features.Countries.Commands.UpdateCountry;

public sealed class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode("COUNTRY_ID_REQUIRED");
            
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("COUNTRY_NAME_REQUIRED")
            .MaximumLength(200).WithErrorCode("COUNTRY_NAME_TOO_LONG");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithErrorCode("COUNTRY_CODE_REQUIRED")
            .Length(2).WithErrorCode("COUNTRY_CODE_LENGTH")
            .Matches("^[A-Z]{2}$").WithErrorCode("COUNTRY_CODE_FORMAT");

        RuleFor(x => x.PhoneCode)
            .NotEmpty().WithErrorCode("PHONE_CODE_REQUIRED")
            .MaximumLength(10).WithErrorCode("PHONE_CODE_TOO_LONG");
    }
}
