using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HRMS.API.Common;
using HRMS.API.IntegrationTests.Setup;
using HRMS.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace HRMS.API.IntegrationTests.Setup;

public abstract class IntegrationTestBase : IClassFixture<HrmsWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HrmsWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(HrmsWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        
        // Setup default headers for Tenant and Auth
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", HrmsWebApplicationFactory.TestTenantId.ToString());
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateTestJwt());
    }

    public virtual async Task InitializeAsync()
    {
        // For InMemory databases, Respawn isn't compatible. We'll simply clear the database manually.
        using var scope = Factory.Services.CreateScope();
        
        // Clear Tenant 1
        var tenantService = scope.ServiceProvider.GetRequiredService<HRMS.Infrastructure.MultiTenancy.CurrentTenantService>();
        tenantService.SetCurrentTenantId(HrmsWebApplicationFactory.TestTenantId);
        
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Countries.RemoveRange(db.Countries);
        await db.SaveChangesAsync();

        // Clear Tenant 2
        // Since the DB is scoped, we might need a new scope for a new tenant ID due to the check
        // "Tenant ID cannot be changed once set in the current scope" in CurrentTenantService.
        // Let's use a new scope for Tenant 2.
        using var scope2 = Factory.Services.CreateScope();
        var tenantService2 = scope2.ServiceProvider.GetRequiredService<HRMS.Infrastructure.MultiTenancy.CurrentTenantService>();
        tenantService2.SetCurrentTenantId(HrmsWebApplicationFactory.TestTenant2Id);
        
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db2.Countries.RemoveRange(db2.Countries);
        await db2.SaveChangesAsync();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected void SetTenantId(Guid tenantId)
    {
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }
    
    protected void RemoveTenantHeader()
    {
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
    }

    protected string GenerateTestJwt()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretTestingKeyThatIsAtLeast32BytesLong!"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: new[] { new Claim(ClaimTypes.Name, "TestUser") },
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data)
    {
        return await Client.PostAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data)
    {
        return await Client.PutAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }
}
