using HRMS.Domain.Common;
using HRMS.Domain.Events;
using HRMS.Domain.ValueObjects;

namespace HRMS.Domain.Entities;

/// <summary>
/// Country Aggregate Root — master data entity representing a country within a tenant scope.
/// All mutations go through explicit methods that enforce invariants and raise domain events.
/// </summary>
public sealed class Country : BaseEntity<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public CountryCode CountryCode { get; private set; } = null!;
    public string PhoneCode { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    /// <summary>EF Core materialization constructor.</summary>
    private Country()
    {
    }

    private Country(Guid id, Guid tenantId, string name, CountryCode countryCode, string phoneCode)
        : base(id, tenantId)
    {
        Name = name;
        CountryCode = countryCode;
        PhoneCode = phoneCode;
        IsActive = true;
    }

    /// <summary>
    /// Factory method — the only way to create a Country aggregate.
    /// Validates invariants and raises <see cref="CountryCreatedEvent"/>.
    /// </summary>
    public static Country Create(Guid tenantId, string name, string countryCode, string phoneCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Country name cannot be empty.");

        if (string.IsNullOrWhiteSpace(phoneCode))
            throw new DomainException("Phone code cannot be empty.");

        var country = new Country(
            Guid.NewGuid(),
            tenantId,
            name.Trim(),
            new CountryCode(countryCode),
            phoneCode.Trim());

        country.AddDomainEvent(new CountryCreatedEvent(country.Id, country.Name, countryCode));

        return country;
    }

    /// <summary>
    /// Mutates the aggregate state. Validates invariants and raises <see cref="CountryUpdatedEvent"/>.
    /// </summary>
    public void Update(string name, string countryCode, string phoneCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Country name cannot be empty.");

        if (string.IsNullOrWhiteSpace(phoneCode))
            throw new DomainException("Phone code cannot be empty.");

        Name = name.Trim();
        CountryCode = new CountryCode(countryCode);
        PhoneCode = phoneCode.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CountryUpdatedEvent(Id, Name, countryCode));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
