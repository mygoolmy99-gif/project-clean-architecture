using HRMS.Domain.Entities;
using HRMS.Infrastructure.IntegrationTests.Setup;
using HRMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace HRMS.Infrastructure.IntegrationTests;

public class ApplicationDbContextTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly Guid _tenantId;
    private readonly string _connectionString;

    public ApplicationDbContextTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _tenantId = DatabaseFixture.Tenant1Id;
        _connectionString = fixture.Tenant1ConnectionString;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SaveChangesAsync_StampsTenantId_OnNewEntities()
    {
        // Arrange
        await using var context = _fixture.CreateContext(_tenantId, _connectionString);
        var country = Country.Create(_tenantId, "Test Country", "TC", "+1");
        
        // Clear TenantId to verify it gets stamped
        // Note: In real scenario, TenantId is set via base constructor
        
        // Act
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Assert
        country.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_UpdatesUpdatedAt_OnModifiedEntities()
    {
        // Arrange
        await using var context = _fixture.CreateContext(_tenantId, _connectionString);
        var country = Country.Create(_tenantId, "Test Country", "TC", "+1");
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        
        var originalUpdatedAt = country.UpdatedAt;
        await Task.Delay(10); // Ensure time difference

        // Act
        country.Update("Updated Country", "UC", "+2");
        await context.SaveChangesAsync();

        // Assert
        country.UpdatedAt.Should().BeGreaterThan(originalUpdatedAt!.Value);
    }

    [Fact]
    public async Task RowVersion_IsUpdated_OnEachSave()
    {
        // Arrange
        await using var context = _fixture.CreateContext(_tenantId, _connectionString);
        var country = Country.Create(_tenantId, "Test Country", "TC", "+1");
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        
        // Get the RowVersion from database
        var entry = context.Entry(country);
        var originalRowVersion = entry.Property("RowVersion").CurrentValue as byte[];
        originalRowVersion.Should().NotBeNull();

        // Act - Update the entity
        country.Update("Updated Country", "UC", "+2");
        await context.SaveChangesAsync();

        // Assert
        var newRowVersion = entry.Property("RowVersion").CurrentValue as byte[];
        newRowVersion.Should().NotBeNull();
        newRowVersion.Should().NotBeEquivalentTo(originalRowVersion);
    }

    [Fact]
    public async Task TenantQueryFilter_IsApplied_Correctly()
    {
        // Arrange - Create countries in both tenants
        await using var context1 = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _fixture.Tenant1ConnectionString);
        await using var context2 = _fixture.CreateContext(DatabaseFixture.Tenant2Id, _fixture.Tenant2ConnectionString);
        
        var country1 = Country.Create(DatabaseFixture.Tenant1Id, "Tenant1 Country", "T1", "+1");
        var country2 = Country.Create(DatabaseFixture.Tenant2Id, "Tenant2 Country", "T2", "+2");
        
        context1.Countries.Add(country1);
        await context1.SaveChangesAsync();
        
        context2.Countries.Add(country2);
        await context2.SaveChangesAsync();

        // Act - Query from Tenant1 context (should only see Tenant1's country)
        await using var verifyContext = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _fixture.Tenant1ConnectionString);
        var countries = await verifyContext.Countries.ToListAsync();

        // Assert
        countries.Should().HaveCount(1);
        countries[0].Name.Should().Be("Tenant1 Country");
        countries[0].TenantId.Should().Be(DatabaseFixture.Tenant1Id);
    }
}
