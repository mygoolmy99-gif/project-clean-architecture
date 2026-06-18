using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.MultiTenancy;

public sealed class HeaderTenantResolver(ITenantStore tenantStore) : ITenantResolver
{
    private const string TenantIdHeaderName = "X-Tenant-Id";

    public async Task<TenantInfo?> ResolveAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeaderName, out var values))
        {
            var headerValue = values.FirstOrDefault();
            if (Guid.TryParse(headerValue, out var tenantId))
            {
                return await tenantStore.GetTenantAsync(tenantId);
            }
        }

        return null;
    }
}
