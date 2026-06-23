using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Designations.Commands.UpdateDesignation;

/// <summary>
/// STEP 16: Update Designation Command
/// Command to update an existing Designation aggregate.
/// 
/// Purpose:
/// - Defines the data required to update a designation
/// - Implements IRequest<Result<Guid>> to use MediatR CQRS pattern
/// - Returns the ID of the updated designation
/// 
/// Usage: Sent via MediatR ISender to trigger the UpdateDesignationCommandHandler
/// </summary>
public sealed record UpdateDesignationCommand(
    Guid Id,      // The ID of the designation to update
    string Name   // The new name for the designation
) : IRequest<Result<Guid>>;
