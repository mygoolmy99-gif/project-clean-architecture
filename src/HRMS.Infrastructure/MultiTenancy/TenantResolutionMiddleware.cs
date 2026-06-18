using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure.MultiTenancy;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantResolver = context.RequestServices.GetRequiredService<ITenantResolver>();
        var currentTenantService = context.RequestServices.GetRequiredService<CurrentTenantService>();

        var tenant = await tenantResolver.ResolveAsync(context);

        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"Error\": \"Tenant ID is required and must be valid.\"}");
            return;
        }

        currentTenantService.SetCurrentTenantId(tenant.Id);
        currentTenantService.ConnectionString = tenant.ConnectionString;

        await next(context);
    }
}
