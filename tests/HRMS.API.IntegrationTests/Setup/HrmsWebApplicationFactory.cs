using HRMS.Infrastructure.MultiTenancy;
using HRMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HRMS.API.IntegrationTests.Setup;

public class HrmsWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid TestTenant2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration settings for tests
            var testConfig = new Dictionary<string, string?>
            {
                { "Tenants:0:Id", TestTenantId.ToString() },
                { "Tenants:0:Name", "Test Tenant 1" },
                { "Tenants:0:ConnectionString", "" },
                { "Tenants:1:Id", TestTenant2Id.ToString() },
                { "Tenants:1:Name", "Test Tenant 2" },
                { "Tenants:1:ConnectionString", "" },
                { "Jwt:Key", "SuperSecretTestingKeyThatIsAtLeast32BytesLong!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            
            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            
            // Add InMemory Database
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var tenantService = sp.GetRequiredService<CurrentTenantService>();
                var dbName = "InMemory_Default";
                try
                {
                    dbName = "InMemory_" + tenantService.GetCurrentTenantId().ToString();
                }
                catch
                {
                    // Ignore and use default
                }
                options.UseInMemoryDatabase(dbName);
            });
        });
    }
}
