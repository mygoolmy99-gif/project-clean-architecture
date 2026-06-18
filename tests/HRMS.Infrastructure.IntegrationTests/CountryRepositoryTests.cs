using HRMS.Domain.Entities;
using HRMS.Domain.Repositories;
using HRMS.Infrastructure.IntegrationTests.Setup;
using HRMS.Infrastructure.Persistence;
using HRMS.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace HRMS.Infrastructure.IntegrationTests;

public class CountryRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly string _tenant1ConnectionString;
    private readonly string _tenant2ConnectionString;

    public CountryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _tenant1ConnectionString = fixture.Tenant1ConnectionString;
        _tenant2ConnectionString = fixture.Tenant2ConnectionString;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_SavesEntityToDatabase()
    {
        // Arrange
        await using var context = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repository = new CountryRepository(context);
        var country = Country.Create(DatabaseFixture.Tenant1Id, "New Country", "NC", "+999");

        // Act
        await repository.AddAsync(country);
        await context.SaveChangesAsync();

        // Assert
        country.Id.Should().NotBe(Guid.Empty);
        
        // Verify by querying directly
        var savedCountry = await context.Countries.FindAsync(country.Id);
        savedCountry.Should().NotBeNull();
        savedCountry!.Name.Should().Be("New Country");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntityWithCorrectData()
    {
        // Arrange
        await using var context = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repository = new CountryRepository(context);
        var country = Country.Create(DatabaseFixture.Tenant1Id, "Lookup Country", "LC", "+888");
        
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Act
        var retrieved = await repository.GetByIdAsync(country.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Lookup Country");
        retrieved.CountryCode.Value.Should().Be("LC");
        retrieved.PhoneCode.Should().Be("+888");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntitiesForCurrentTenant()
    {
        // Arrange
        await using var context = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repository = new CountryRepository(context);
        
        var country1 = Country.Create(DatabaseFixture.Tenant1Id, "Country 1", "C1", "+1");
        var country2 = Country.Create(DatabaseFixture.Tenant1Id, "Country 2", "C2", "+2");
        var country3 = Country.Create(DatabaseFixture.Tenant1Id, "Country 3", "C3", "+3");
        
        context.Countries.AddRange(country1, country2, country3);
        await context.SaveChangesAsync();

        // Act
        var countries = await repository.GetAllAsync();

        // Assert
        countries.Should().HaveCount(3);
        countries.Select(c => c.Name).Should().Contain(new[] { "Country 1", "Country 2", "Country 3" });
    }

    [Fact]
    public async Task UpdateAsync_ModifiesEntity()
    {
        // Arrange
        await using var context = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repository = new CountryRepository(context);
        var country = Country.Create(DatabaseFixture.Tenant1Id, "Original Name", "ON", "+777");
        
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Act
        country.Update("Updated Name", "UN", "+666");
        repository.Update(country);
        await context.SaveChangesAsync();

        // Assert
        var updated = await repository.GetByIdAsync(country.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.CountryCode.Value.Should().Be("UN");
        updated.PhoneCode.Should().Be("+666");
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity()
    {
        // Arrange
        await using var context = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repository = new CountryRepository(context);
        var country = Country.Create(DatabaseFixture.Tenant1Id, "ToDelete", "TD", "+555");
        
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        // Act
        repository.Delete(country);
        await context.SaveChangesAsync();

        // Assert
        var deleted = await repository.GetByIdAsync(country.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task TenantIsolation_EntityFromTenantA_NotVisibleInTenantB()
    {
        // Arrange - Create country in Tenant A
        await using var contextA = _fixture.CreateContext(DatabaseFixture.Tenant1Id, _tenant1ConnectionString);
        var repositoryA = new CountryRepository(contextA);
        var countryA = Country.Create(DatabaseFixture.Tenant1Id, "Tenant A Country", "TA", "+1");
        
        contextA.Countries.Add(countryA);
        await contextA.SaveChangesAsync();

        // Act - Try to retrieve from Tenant B context
        await using var contextB = _fixture.CreateContext(DatabaseFixture.Tenant2Id, _tenant2ConnectionString);
        var repositoryB = new CountryRepository(contextB);
        
        // Query all countries in Tenant B
        var tenantBCountries = await repositoryB.GetAllAsync();
        
        // Try to get by ID (should return null due to tenant filter)
        var retrievedById = await repositoryB.GetByIdAsync(countryA.Id);

        // Assert
        tenantBCountries.Should().BeEmpty("Tenant B should not see Tenant A's countries");
        retrievedById.Should().BeNull("Tenant B should not be able to retrieve Tenant A's country by ID");
    }
}
