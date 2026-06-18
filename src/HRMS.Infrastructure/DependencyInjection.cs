using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.MultiTenancy;
using HRMS.Infrastructure.Persistence;
using HRMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Multi-Tenancy
        services.AddSingleton<ITenantStore, InMemoryTenantStore>();
        services.AddScoped<ITenantResolver, HeaderTenantResolver>();
        services.AddScoped<CurrentTenantService>();
        services.AddScoped<ICurrentTenantService>(sp => sp.GetRequiredService<CurrentTenantService>());

        // Persistence
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var tenantService = sp.GetRequiredService<CurrentTenantService>();
            if (!string.IsNullOrEmpty(tenantService.ConnectionString))
            {
                options.UseSqlServer(tenantService.ConnectionString);
            }
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICountryRepository, CountryRepository>();
        
        services.AddTransient<MultiTenantMigrator>();

        return services;
    }
}
