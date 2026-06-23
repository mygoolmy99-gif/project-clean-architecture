using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Repositories;
using MediatR;

namespace HRMS.Application.Features.Designations.Commands.UpdateDesignation;

/// <summary>
/// STEP 18: Update Designation Command Handler
/// Handles the UpdateDesignationCommand and executes the business logic.
/// 
/// Purpose:
/// - Orchestrates the update of an existing designation
/// - Retrieves the designation from repository
/// - Calls domain method to update entity
/// - Persists changes via repository and unit of work
/// 
/// Dependencies:
/// - IDesignationRepository: For data access
/// - IUnitOfWork: For transaction management
/// </summary>
public sealed class UpdateDesignationCommandHandler(
    IDesignationRepository designationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDesignationCommand, Result<Guid>>
{
    /// <summary>
    /// Handles the UpdateDesignationCommand
    /// STEP 19: Execute Business Logic
    /// 
    /// Process:
    /// 1. Retrieve designation by ID from repository
    /// 2. Check if designation exists (return NotFound if not)
    /// 3. Call domain update method on entity
    /// 4. Update designation in repository
    /// 5. Save changes to database
    /// 6. Return the updated designation ID
    /// </summary>
    public async Task<Result<Guid>> Handle(UpdateDesignationCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Retrieve designation by ID
        var designation = await designationRepository.GetByIdAsync(request.Id, cancellationToken);
        
        // Step 2: Check if designation exists
        if (designation == null)
        {
            return Result<Guid>.NotFound($"Designation with ID {request.Id} not found.");
        }
        
        // Step 3: Call domain update method (enforces business rules, raises events)
        designation.Update(request.Name);
        
        // Step 4: Mark for update in repository
        designationRepository.Update(designation);
        
        // Step 5: Persist changes to database
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Step 6: Return the updated designation ID
        return designation.Id;
    }
}
