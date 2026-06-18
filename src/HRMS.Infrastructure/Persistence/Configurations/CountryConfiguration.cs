using HRMS.Domain.Entities;
using HRMS.Domain.ValueObjects;
using HRMS.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CountryCode)
            .HasConversion(
                v => v.Value,
                v => new CountryCode(v))
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.PhoneCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasIndex(c => new { c.TenantId, c.CountryCode }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.IsActive });
        
        builder.HasQueryFilter(e => e.TenantId == EF.Property<Guid>(e, "TenantId"));
    }
}
