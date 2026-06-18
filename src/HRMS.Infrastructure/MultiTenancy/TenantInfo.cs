namespace HRMS.Infrastructure.MultiTenancy;

public sealed record TenantInfo(Guid Id, string Name, string ConnectionString);
