namespace HRMS.Infrastructure.MultiTenancy;

public interface ITenantStore
{
    Task<TenantInfo?> GetTenantAsync(Guid tenantId);
    Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync();
}
