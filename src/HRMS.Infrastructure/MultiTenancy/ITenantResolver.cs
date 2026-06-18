using Microsoft.AspNetCore.Http;

namespace HRMS.Infrastructure.MultiTenancy;

public interface ITenantResolver
{
    Task<TenantInfo?> ResolveAsync(HttpContext context);
}
