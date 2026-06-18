using System.Net;
using HRMS.Infrastructure.IntegrationTests.Setup;
using HRMS.Infrastructure.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using Xunit;

namespace HRMS.Infrastructure.IntegrationTests;

public class MultiTenancyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MultiTenancyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HeaderTenantResolver_ExtractsTenant_FromXTenantIdHeader()
    {
        // Arrange
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Tenants:0:Id", DatabaseFixture.Tenant1Id.ToString() },
                { "Tenants:0:Name", "Test Tenant 1" },
                { "Tenants:0:ConnectionString", _fixture.Tenant1ConnectionString }
            })!);
        
        var resolver = new HeaderTenantResolver(tenantStore);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = DatabaseFixture.Tenant1Id.ToString();

        // Act
        var tenant = await resolver.ResolveAsync(httpContext);

        // Assert
        tenant.Should().NotBeNull();
        tenant!.Id.Should().Be(DatabaseFixture.Tenant1Id);
        tenant.Name.Should().Be("Test Tenant 1");
    }

    [Fact]
    public async Task HeaderTenantResolver_ReturnsNull_ForMissingHeader()
    {
        // Arrange
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())!);
        
        var resolver = new HeaderTenantResolver(tenantStore);
        
        var httpContext = new DefaultHttpContext();
        // No X-Tenant-Id header

        // Act
        var tenant = await resolver.ResolveAsync(httpContext);

        // Assert
        tenant.Should().BeNull();
    }

    [Fact]
    public async Task HeaderTenantResolver_ReturnsNull_ForInvalidGuid()
    {
        // Arrange
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())!);
        
        var resolver = new HeaderTenantResolver(tenantStore);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = "not-a-valid-guid";

        // Act
        var tenant = await resolver.ResolveAsync(httpContext);

        // Assert
        tenant.Should().BeNull();
    }

    [Fact]
    public async Task TenantResolutionMiddleware_SetsCurrentTenantService()
    {
        // Arrange
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Tenants:0:Id", DatabaseFixture.Tenant1Id.ToString() },
                { "Tenants:0:Name", "Test Tenant 1" },
                { "Tenants:0:ConnectionString", _fixture.Tenant1ConnectionString }
            })!);
        
        var resolver = new HeaderTenantResolver(tenantStore);
        var currentTenantService = new CurrentTenantService();
        
        var services = new ServiceCollection();
        services.AddSingleton<ITenantResolver>(resolver);
        services.AddSingleton(currentTenantService);
        var serviceProvider = services.BuildServiceProvider();
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Id"] = DatabaseFixture.Tenant1Id.ToString();
        httpContext.RequestServices = serviceProvider;

        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        currentTenantService.GetCurrentTenantId().Should().Be(DatabaseFixture.Tenant1Id);
        currentTenantService.ConnectionString.Should().Be(_fixture.Tenant1ConnectionString);
    }

    [Fact]
    public async Task TenantResolutionMiddleware_Returns400_ForMissingTenant()
    {
        // Arrange
        var tenantStore = new InMemoryTenantStore(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())!);
        
        var resolver = new HeaderTenantResolver(tenantStore);
        var currentTenantService = new CurrentTenantService();
        
        var services = new ServiceCollection();
        services.AddSingleton<ITenantResolver>(resolver);
        services.AddSingleton(currentTenantService);
        var serviceProvider = services.BuildServiceProvider();
        
        var httpContext = new DefaultHttpContext();
        // No X-Tenant-Id header
        httpContext.RequestServices = serviceProvider;

        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void DynamicConnectionStringSwitching_WorksCorrectly()
    {
        // Arrange
        var currentTenantService = new CurrentTenantService();
        
        // Act & Assert - Switch between tenants
        currentTenantService.SetCurrentTenantId(DatabaseFixture.Tenant1Id);
        currentTenantService.ConnectionString = _fixture.Tenant1ConnectionString;
        currentTenantService.GetCurrentTenantId().Should().Be(DatabaseFixture.Tenant1Id);
        currentTenantService.ConnectionString.Should().Be(_fixture.Tenant1ConnectionString);
        
        // Verify cannot change tenant ID once set (in same scope)
        var act = () => currentTenantService.SetCurrentTenantId(DatabaseFixture.Tenant2Id);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Tenant ID cannot be changed once set*");
    }
}
