using System.Reflection;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly CurrentTenantService _currentTenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        CurrentTenantService currentTenantService) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Country> Countries => Set<Country>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _currentTenantService.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenantService.GetCurrentTenantId();

        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(e => e.TenantId).CurrentValue = tenantId;
                    entry.Property(e => e.CreatedAt).CurrentValue = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(e => e.TenantId).CurrentValue = tenantId;
                    entry.Property(e => e.UpdatedAt).CurrentValue = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
