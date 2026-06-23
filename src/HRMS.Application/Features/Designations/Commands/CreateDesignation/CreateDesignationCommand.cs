using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Designations.Commands.CreateDesignation;

/// <summary>
/// STEP 12: Create Designation Command
/// Command to create a new Designation aggregate within the current tenant scope.
/// 
/// Purpose:
/// - Defines the data required to create a designation
/// - Implements IRequest<Result<Guid>> to use MediatR CQRS pattern
/// - Returns the ID of the created designation
/// 
/// Usage: Sent via MediatR ISender to trigger the CreateDesignationCommandHandler
/// </summary>
public sealed record CreateDesignationCommand(
    string Name) : IRequest<Result<Guid>>;
