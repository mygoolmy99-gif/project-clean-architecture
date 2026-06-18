namespace HRMS.Application.Common.Interfaces;

/// <summary>
/// Provides the current tenant's identity for multi-tenancy scoping.
/// Implemented by Infrastructure (resolved from HTTP context, JWT claims, etc.).
/// </summary>
public interface ICurrentTenantService
{
    Guid GetCurrentTenantId();
}
