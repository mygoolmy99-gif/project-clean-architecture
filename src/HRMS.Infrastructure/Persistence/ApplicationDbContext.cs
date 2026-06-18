using System.Reflection;
using System.Linq.Expressions;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply tenant filter to all BaseEntity<T> types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity<Guid>).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilterExpression(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private LambdaExpression CreateTenantFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var tenantIdProperty = Expression.Property(parameter, "TenantId");
        var currentTenantId = Expression.Call(
            Expression.Constant(_currentTenantService),
            typeof(ICurrentTenantService).GetMethod("GetCurrentTenantId")!
        );
        var body = Expression.Equal(tenantIdProperty, currentTenantId);
        return Expression.Lambda(body, parameter);
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
