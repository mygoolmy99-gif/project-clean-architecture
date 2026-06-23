using FluentValidation;

namespace HRMS.Application.Features.Designations.Commands.CreateDesignation;

/// <summary>
/// STEP 13: Create Designation Command Validator
/// Validates the CreateDesignationCommand using FluentValidation.
/// 
/// Purpose:
/// - Ensures command data meets business rules before processing
/// - Provides clear error messages for validation failures
/// - Automatically executed by MediatR pipeline behavior
/// 
/// Validation Rules:
/// - Name: Required, cannot be empty, max 200 characters
/// </summary>
public sealed class CreateDesignationCommandValidator : AbstractValidator<CreateDesignationCommand>
{
    public CreateDesignationCommandValidator()
    {
        // Rule 1: Name is required
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("DESIGNATION_NAME_REQUIRED")
            .WithMessage("Designation name is required.")
            .MaximumLength(200).WithErrorCode("DESIGNATION_NAME_TOO_LONG")
            .WithMessage("Designation name cannot exceed 200 characters.");
    }
}
