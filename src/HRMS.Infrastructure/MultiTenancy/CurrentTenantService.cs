using HRMS.Application.Common.Interfaces;

namespace HRMS.Infrastructure.MultiTenancy;

public sealed class CurrentTenantService : ICurrentTenantService
{
    private Guid _currentTenantId;

    public void SetCurrentTenantId(Guid tenantId)
    {
        if (_currentTenantId != Guid.Empty && _currentTenantId != tenantId)
        {
            throw new InvalidOperationException("Tenant ID cannot be changed once set in the current scope.");
        }

        _currentTenantId = tenantId;
    }

    public Guid GetCurrentTenantId()
    {
        if (_currentTenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant ID is not resolved in the current scope.");
        }

        return _currentTenantId;
    }
    
    public string? ConnectionString { get; set; }
}
