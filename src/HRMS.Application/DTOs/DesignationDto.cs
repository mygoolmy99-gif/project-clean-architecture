namespace HRMS.Application.DTOs;

/// <summary>
/// STEP 11: Data Transfer Object (DTO) for Designation
/// Read-only projection of the Designation aggregate for query responses.
/// 
/// Purpose:
/// - Defines the shape of data returned to API consumers
/// - Decouples internal domain model from external API contracts
/// - Contains only the fields needed for read operations
/// 
/// Usage: Returned by GET endpoints for designation data
/// </summary>
public sealed record DesignationDto(
    Guid Id,                    // Unique identifier for the designation
    string Name,                // Designation name (e.g., "Software Engineer")
    bool IsActive,              // Whether the designation is active
    DateTime CreatedAt,         // When the designation was created
    DateTime? UpdatedAt,        // When the designation was last updated
    byte[] RowVersion);         // Concurrency token for optimistic locking
