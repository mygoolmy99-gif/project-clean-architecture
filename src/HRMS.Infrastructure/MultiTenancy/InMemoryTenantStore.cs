using Microsoft.Extensions.Configuration;

namespace HRMS.Infrastructure.MultiTenancy;

public sealed class InMemoryTenantStore : ITenantStore
{
    private readonly Dictionary<Guid, TenantInfo> _tenants;

    public InMemoryTenantStore(IConfiguration configuration)
    {
        _tenants = new Dictionary<Guid, TenantInfo>();
        var tenantsSection = configuration.GetSection("Tenants").GetChildren();

        foreach (var section in tenantsSection)
        {
            if (Guid.TryParse(section["Id"], out var id))
            {
                var name = section["Name"] ?? $"Tenant_{id}";
                var connectionString = section["ConnectionString"];
                
                if (connectionString != null)
                {
                    _tenants[id] = new TenantInfo(id, name, connectionString);
                }
            }
        }
    }

    public Task<TenantInfo?> GetTenantAsync(Guid tenantId)
    {
        _tenants.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task<IReadOnlyList<TenantInfo>> GetAllTenantsAsync()
    {
        return Task.FromResult<IReadOnlyList<TenantInfo>>(_tenants.Values.ToList());
    }
}
