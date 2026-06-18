using HRMS.Domain.Common;

namespace HRMS.Domain.ValueObjects;

/// <summary>
/// Strongly-typed value object wrapping a Tenant identifier.
/// Rejects <see cref="Guid.Empty"/> to prevent accidental unscoped data access.
/// </summary>
public sealed class TenantId : ValueObject
{
    public Guid Value { get; }

    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException("TenantId cannot be empty.");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;

    public static explicit operator TenantId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
