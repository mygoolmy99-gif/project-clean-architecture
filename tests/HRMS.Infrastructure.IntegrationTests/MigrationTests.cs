using HRMS.Infrastructure.IntegrationTests.Setup;
using HRMS.Infrastructure.MultiTenancy;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace HRMS.Infrastructure.IntegrationTests;

public class MigrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MigrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MultiTenantMigrator_AppliesMigrations_ToAllTenants()
    {
        // Arrange - Create a fresh database for this test
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = "CREATE DATABASE HRMS_MigrationTest";
        await createCommand.ExecuteNonQueryAsync();
        
        var testConnectionString = _fixture.ConnectionString.Replace("Database=master", "Database=HRMS_MigrationTest");
        
        // Setup tenant store with test tenants
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Tenants:0:Id", DatabaseFixture.Tenant1Id.ToString() },
                { "Tenants:0:Name", "Migration Test Tenant 1" },
                { "Tenants:0:ConnectionString", testConnectionString },
                { "Tenants:1:Id", DatabaseFixture.Tenant2Id.ToString() },
                { "Tenants:1:Name", "Migration Test Tenant 2" },
                { "Tenants:1:ConnectionString", testConnectionString }
            })!);
        
        // Setup service provider
        var services = new ServiceCollection();
        services.AddSingleton<ITenantStore>(tenantStore);
        services.AddSingleton<CurrentTenantService>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var tenantService = sp.GetRequiredService<CurrentTenantService>();
            var cs = tenantService.ConnectionString ?? testConnectionString;
            options.UseSqlServer(cs);
        });
        services.AddSingleton<MultiTenantMigrator>();
        services.AddLogging(builder => builder.AddConsole());
        
        var serviceProvider = services.BuildServiceProvider();
        var migrator = serviceProvider.GetRequiredService<MultiTenantMigrator>();

        // Act
        await migrator.MigrateAllAsync();

        // Assert - Verify migrations were applied to both tenant databases
        await using var context1 = _fixture.CreateContext(DatabaseFixture.Tenant1Id, testConnectionString);
        await using var context2 = _fixture.CreateContext(DatabaseFixture.Tenant2Id, testConnectionString);
        
        // Check that Countries table exists and is accessible
        var countries1 = await context1.Countries.ToListAsync();
        var countries2 = await context2.Countries.ToListAsync();
        
        countries1.Should().NotBeNull("Countries table should exist in Tenant 1 database");
        countries2.Should().NotBeNull("Countries table should exist in Tenant 2 database");
        
        // Cleanup
        await DropDatabaseAsync("HRMS_MigrationTest");
    }

    [Fact]
    public async Task MigrationFailure_OnOneTenant_DoesNotAffectOthers()
    {
        // Arrange - Create test databases
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = "CREATE DATABASE HRMS_FailureTest";
        await createCommand.ExecuteNonQueryAsync();
        
        var testConnectionString = _fixture.ConnectionString.Replace("Database=master", "Database=HRMS_FailureTest");
        
        // Create a tenant store where one tenant has an invalid connection string
        var tenantStoreMock = new Mock<ITenantStore>();
        tenantStoreMock.Setup(x => x.GetAllTenantsAsync()).ReturnsAsync(new List<TenantInfo>
        {
            new(DatabaseFixture.Tenant1Id, "Valid Tenant", testConnectionString),
            new(DatabaseFixture.Tenant2Id, "Invalid Tenant", "Server=invalid;Database=nonexistent;Trusted_Connection=True;")
        });
        
        // Setup service provider
        var services = new ServiceCollection();
        services.AddSingleton<ITenantStore>(tenantStoreMock.Object);
        services.AddSingleton<CurrentTenantService>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var tenantService = sp.GetRequiredService<CurrentTenantService>();
            var cs = tenantService.ConnectionString ?? testConnectionString;
            options.UseSqlServer(cs, sql => sql.EnableRetryOnFailure(0)); // No retries for faster failure
        });
        services.AddSingleton<MultiTenantMigrator>();
        services.AddLogging(builder => builder.AddConsole());
        
        var serviceProvider = services.BuildServiceProvider();
        var migrator = serviceProvider.GetRequiredService<MultiTenantMigrator>();

        // Act & Assert - Migration should throw but first tenant should still be migrated
        await Assert.ThrowsAnyAsync<Exception>(async () => await migrator.MigrateAllAsync());
        
        // Verify first tenant was migrated successfully before the failure
        await using var context1 = _fixture.CreateContext(DatabaseFixture.Tenant1Id, testConnectionString);
        var countries = await context1.Countries.ToListAsync();
        countries.Should().NotBeNull("First tenant should have been migrated before failure on second tenant");
        
        // Cleanup
        await DropDatabaseAsync("HRMS_FailureTest");
    }

    private async Task DropDatabaseAsync(string dbName)
    {
        try
        {
            await using var connection = new SqlConnection(_fixture.ConnectionString);
            await connection.OpenAsync();
            
            // Close any active connections to the database
            await using var killCommand = connection.CreateCommand();
            killCommand.CommandText = $@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += N'KILL ' + CAST(spid AS NVARCHAR(10)) + N';'
                FROM sys.sysprocesses 
                WHERE dbid = DB_ID('{dbName}');
                EXEC(@sql);";
            await killCommand.ExecuteNonQueryAsync();
            
            await using var dropCommand = connection.CreateCommand();
            dropCommand.CommandText = $"DROP DATABASE {dbName}";
            await dropCommand.ExecuteNonQueryAsync();
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }
}
