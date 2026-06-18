using Testcontainers.MsSql;
using HRMS.Infrastructure.MultiTenancy;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.IntegrationTests.Setup;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("Your_password123!")
        .Build();

    public string ConnectionString => _container.GetConnectionString();
    public string Tenant1ConnectionString { get; private set; } = string.Empty;
    public string Tenant2ConnectionString { get; private set; } = string.Empty;
    
    public static readonly Guid Tenant1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Tenant2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        // Create test tenant databases
        await CreateDatabaseAsync("HRMS_Tenant1");
        await CreateDatabaseAsync("HRMS_Tenant2");
        
        Tenant1ConnectionString = ConnectionString.Replace("Database=master", "Database=HRMS_Tenant1");
        Tenant2ConnectionString = ConnectionString.Replace("Database=master", "Database=HRMS_Tenant2");
        
        // Apply migrations to both tenant databases
        await ApplyMigrationsAsync(Tenant1ConnectionString, Tenant1Id);
        await ApplyMigrationsAsync(Tenant2ConnectionString, Tenant2Id);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }

    private async Task CreateDatabaseAsync(string dbName)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {dbName}";
        await command.ExecuteNonQueryAsync();
    }

    private async Task ApplyMigrationsAsync(string connectionString, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new ApplicationDbContext(options, new CurrentTenantService());
        await context.Database.MigrateAsync();
    }

    public ApplicationDbContext CreateContext(Guid tenantId, string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var currentTenantService = new CurrentTenantService();
        currentTenantService.SetCurrentTenantId(tenantId);
        currentTenantService.ConnectionString = connectionString;

        return new ApplicationDbContext(options, currentTenantService);
    }
}
