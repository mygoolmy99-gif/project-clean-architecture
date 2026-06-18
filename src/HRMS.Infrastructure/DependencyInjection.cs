using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.MultiTenancy;
using HRMS.Infrastructure.Persistence;
using HRMS.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        // Persistence - Use IDbContextFactory pattern for proper tenant resolution
        services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
        {
            var tenantService = sp.GetRequiredService<ICurrentTenantService>();
            var connectionString = tenantService.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Tenant connection string not resolved");
            }
            
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
            });
            
            // Enable sensitive data logging only in Development
            if (sp.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Register DbContext as scoped, created from factory
        services.AddScoped<ApplicationDbContext>(sp => 
            sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICountryRepository, CountryRepository>();
        
        services.AddTransient<MultiTenantMigrator>();

        return services;
    }
}
