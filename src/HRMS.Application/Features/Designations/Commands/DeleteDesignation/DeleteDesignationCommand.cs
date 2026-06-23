using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Features.Designations.Commands.DeleteDesignation;

/// <summary>
/// STEP 20: Delete Designation Command
/// Command to delete (soft delete) an existing Designation aggregate.
/// 
/// Purpose:
/// - Defines the ID of the designation to delete
/// - Implements IRequest<Result<Unit>> to use MediatR CQRS pattern
/// - Returns Unit (void equivalent) for delete operations
/// 
/// Usage: Sent via MediatR ISender to trigger the DeleteDesignationCommandHandler
/// </summary>
public sealed record DeleteDesignationCommand(
    Guid Id) : IRequest<Result<Unit>>;
