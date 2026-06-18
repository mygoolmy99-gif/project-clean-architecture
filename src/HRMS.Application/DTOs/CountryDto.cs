namespace HRMS.Application.DTOs;

/// <summary>
/// Read-only projection of the Country aggregate for query responses.
/// </summary>
public sealed record CountryDto(
    Guid Id,
    string Name,
    string CountryCode,
    string PhoneCode,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    byte[] RowVersion);
