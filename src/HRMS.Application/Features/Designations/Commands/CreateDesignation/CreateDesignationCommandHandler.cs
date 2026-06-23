using HRMS.Application.Common;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using MediatR;

namespace HRMS.Application.Features.Designations.Commands.CreateDesignation;

/// <summary>
/// STEP 14: Create Designation Command Handler
/// Handles the CreateDesignationCommand and executes the business logic.
/// 
/// Purpose:
/// - Orchestrates the creation of a new designation
/// - Gets current tenant ID for multi-tenancy support
/// - Calls domain factory method to create entity
/// - Persists changes via repository and unit of work
/// 
/// Dependencies:
/// - IDesignationRepository: For data access
/// - IUnitOfWork: For transaction management
/// - ICurrentTenantService: For multi-tenancy support
/// </summary>
public sealed class CreateDesignationCommandHandler(
    IDesignationRepository designationRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService) : IRequestHandler<CreateDesignationCommand, Result<Guid>>
{
    /// <summary>
    /// Handles the CreateDesignationCommand
    /// STEP 15: Execute Business Logic
    /// 
    /// Process:
    /// 1. Get current tenant ID from context
    /// 2. Call domain factory method Designation.Create()
    /// 3. Add designation to repository
    /// 4. Save changes to database
    /// 5. Return the created designation ID
    /// </summary>
    public async Task<Result<Guid>> Handle(CreateDesignationCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get current tenant ID (for multi-tenancy)
        var tenantId = currentTenantService.GetCurrentTenantId();
        
        // Step 2: Create designation using domain factory method
        // This enforces business rules and raises domain events
        var designation = Designation.Create(tenantId, request.Name);
        
        // Step 3: Add to repository (marks for insertion)
        await designationRepository.AddAsync(designation, cancellationToken);
        
        // Step 4: Persist changes to database
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Step 5: Return the created designation ID
        return designation.Id;
    }
}
