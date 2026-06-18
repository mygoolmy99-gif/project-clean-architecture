using HRMS.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Persistence;

public sealed class MultiTenantMigrator(
    ITenantStore tenantStore,
    IServiceProvider serviceProvider,
    ILogger<MultiTenantMigrator> logger)
{
    public async Task MigrateAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await tenantStore.GetAllTenantsAsync();
        
        foreach (var tenant in tenants)
        {
            logger.LogInformation("Applying migrations for tenant: {TenantName} ({TenantId})", tenant.Name, tenant.Id);
            
            using var scope = serviceProvider.CreateScope();
            var currentTenantService = scope.ServiceProvider.GetRequiredService<CurrentTenantService>();
            
            // Set context for this tenant
            currentTenantService.SetCurrentTenantId(tenant.Id);
            currentTenantService.ConnectionString = tenant.ConnectionString;
            
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            await dbContext.Database.MigrateAsync(cancellationToken);
            
            logger.LogInformation("Successfully applied migrations for tenant: {TenantName}", tenant.Name);
        }
    }
}
