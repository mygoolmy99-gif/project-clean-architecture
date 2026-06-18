using FluentAssertions;
using HRMS.Domain.Common;
using HRMS.Domain.ValueObjects;

namespace HRMS.Domain.UnitTests.ValueObjects;

public class TenantIdTests
{
    // ── Construction guards ────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidGuid_ShouldSucceed()
    {
        var guid = Guid.NewGuid();

        var tenantId = new TenantId(guid);

        tenantId.Value.Should().Be(guid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowDomainException()
    {
        var act = () => new TenantId(Guid.Empty);

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    // ── Equality ───────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameGuid_ShouldBeEqual()
    {
        var guid = Guid.NewGuid();
        var a = new TenantId(guid);
        var b = new TenantId(guid);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentGuid_ShouldNotBeEqual()
    {
        var a = new TenantId(Guid.NewGuid());
        var b = new TenantId(Guid.NewGuid());

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    // ── Conversions ────────────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnValue()
    {
        var guid = Guid.NewGuid();
        var tenantId = new TenantId(guid);

        Guid result = tenantId;

        result.Should().Be(guid);
    }

    [Fact]
    public void ExplicitConversion_FromGuid_ShouldCreateTenantId()
    {
        var guid = Guid.NewGuid();

        var tenantId = (TenantId)guid;

        tenantId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        var guid = Guid.NewGuid();
        var tenantId = new TenantId(guid);

        tenantId.ToString().Should().Be(guid.ToString());
    }
}
