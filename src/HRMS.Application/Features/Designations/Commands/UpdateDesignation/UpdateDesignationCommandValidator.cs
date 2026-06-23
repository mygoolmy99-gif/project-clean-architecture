using FluentValidation;

namespace HRMS.Application.Features.Designations.Commands.UpdateDesignation;

/// <summary>
/// STEP 17: Update Designation Command Validator
/// Validates the UpdateDesignationCommand using FluentValidation.
/// 
/// Purpose:
/// - Ensures command data meets business rules before processing
/// - Provides clear error messages for validation failures
/// - Automatically executed by MediatR pipeline behavior
/// 
/// Validation Rules:
/// - Id: Required, must be a valid GUID
/// - Name: Required, cannot be empty, max 200 characters
/// </summary>
public sealed class UpdateDesignationCommandValidator : AbstractValidator<UpdateDesignationCommand>
{
    public UpdateDesignationCommandValidator()
    {
        // Rule 1: Id is required and must be valid GUID
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode("DESIGNATION_ID_REQUIRED")
            .WithMessage("Designation ID is required.");

        // Rule 2: Name is required
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("DESIGNATION_NAME_REQUIRED")
            .WithMessage("Designation name is required.")
            .MaximumLength(200).WithErrorCode("DESIGNATION_NAME_TOO_LONG")
            .WithMessage("Designation name cannot exceed 200 characters.");
    }
}
