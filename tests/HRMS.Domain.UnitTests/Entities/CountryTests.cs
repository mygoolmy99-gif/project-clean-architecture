using FluentAssertions;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Events;

namespace HRMS.Domain.UnitTests.Entities;

public class CountryTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    // ── Create (happy path) ────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldReturnCountryWithCorrectProperties()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        country.Should().NotBeNull();
        country.Id.Should().NotBe(Guid.Empty);
        country.TenantId.Should().Be(_tenantId);
        country.Name.Should().Be("India");
        country.CountryCode.Value.Should().Be("IN");
        country.PhoneCode.Should().Be("+91");
        country.IsActive.Should().BeTrue();
        country.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        country.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithValidData_ShouldRaiseCountryCreatedEvent()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        country.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CountryCreatedEvent>()
            .Which.Should().BeEquivalentTo(new
            {
                CountryId = country.Id,
                Name = "India",
                CountryCode = "IN"
            });
    }

    [Fact]
    public void Create_ShouldSetIsActiveTrue()
    {
        var country = Country.Create(_tenantId, "United States", "US", "+1");

        country.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimNameAndPhoneCode()
    {
        var country = Country.Create(_tenantId, "  India  ", "IN", "  +91  ");

        country.Name.Should().Be("India");
        country.PhoneCode.Should().Be("+91");
    }

    // ── Create (invariant violations) ──────────────────────────────────────

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Country.Create(_tenantId, "", "IN", "+91");

        act.Should().Throw<DomainException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowDomainException()
    {
        var act = () => Country.Create(_tenantId, "   ", "IN", "+91");

        act.Should().Throw<DomainException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Create_WithEmptyPhoneCode_ShouldThrowDomainException()
    {
        var act = () => Country.Create(_tenantId, "India", "IN", "");

        act.Should().Throw<DomainException>()
            .WithMessage("*Phone code cannot be empty*");
    }

    [Fact]
    public void Create_WithInvalidCountryCode_ShouldThrowDomainException()
    {
        var act = () => Country.Create(_tenantId, "India", "invalid", "+91");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    // ── Update (happy path) ────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_ShouldModifyProperties()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");
        country.ClearDomainEvents();

        country.Update("Republic of India", "IN", "+091");

        country.Name.Should().Be("Republic of India");
        country.CountryCode.Value.Should().Be("IN");
        country.PhoneCode.Should().Be("+091");
    }

    [Fact]
    public void Update_WithValidData_ShouldRaiseCountryUpdatedEvent()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");
        country.ClearDomainEvents();

        country.Update("Republic of India", "IN", "+091");

        country.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CountryUpdatedEvent>()
            .Which.Should().BeEquivalentTo(new
            {
                CountryId = country.Id,
                Name = "Republic of India",
                CountryCode = "IN"
            });
    }

    [Fact]
    public void Update_ShouldSetUpdatedAt()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        country.Update("Republic of India", "IN", "+091");

        country.UpdatedAt.Should().NotBeNull();
        country.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_ShouldTrimNameAndPhoneCode()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");
        country.ClearDomainEvents();

        country.Update("  Republic of India  ", "IN", "  +091  ");

        country.Name.Should().Be("Republic of India");
        country.PhoneCode.Should().Be("+091");
    }

    // ── Update (invariant violations) ──────────────────────────────────────

    [Fact]
    public void Update_WithEmptyName_ShouldThrowDomainException()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        var act = () => country.Update("", "IN", "+91");

        act.Should().Throw<DomainException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Update_WithEmptyPhoneCode_ShouldThrowDomainException()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        var act = () => country.Update("India", "IN", "");

        act.Should().Throw<DomainException>()
            .WithMessage("*Phone code cannot be empty*");
    }

    [Fact]
    public void Update_WithInvalidCountryCode_ShouldThrowDomainException()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        var act = () => country.Update("India", "bad", "+91");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    // ── Activate / Deactivate ──────────────────────────────────────────────

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        country.Deactivate();

        country.IsActive.Should().BeFalse();
        country.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveTrue()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");
        country.Deactivate();

        country.Activate();

        country.IsActive.Should().BeTrue();
    }

    // ── Domain events collection ───────────────────────────────────────────

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");
        country.DomainEvents.Should().NotBeEmpty();

        country.ClearDomainEvents();

        country.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Create_ThenUpdate_ShouldAccumulateTwoDomainEvents()
    {
        var country = Country.Create(_tenantId, "India", "IN", "+91");

        country.Update("Republic of India", "IN", "+091");

        country.DomainEvents.Should().HaveCount(2);
        country.DomainEvents.First().Should().BeOfType<CountryCreatedEvent>();
        country.DomainEvents.Last().Should().BeOfType<CountryUpdatedEvent>();
    }
}
