using FluentAssertions;
using HRMS.Domain.Common;
using HRMS.Domain.ValueObjects;

namespace HRMS.Domain.UnitTests.ValueObjects;

public class CountryCodeTests
{
    // ── Valid codes ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("US")]
    [InlineData("IN")]
    [InlineData("GB")]
    [InlineData("JP")]
    [InlineData("DE")]
    public void Create_WithValidIsoAlpha2Code_ShouldSucceed(string code)
    {
        var countryCode = new CountryCode(code);

        countryCode.Value.Should().Be(code);
    }

    // ── Invalid codes ──────────────────────────────────────────────────────

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("");

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("  ");

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithNull_ShouldThrowDomainException()
    {
        var act = () => new CountryCode(null!);

        act.Should().Throw<DomainException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithLowercaseCode_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("us");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    [Fact]
    public void Create_WithThreeLetterCode_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("USA");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    [Fact]
    public void Create_WithSingleLetter_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("U");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    [Fact]
    public void Create_WithDigits_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("12");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    [Fact]
    public void Create_WithMixedAlphaDigit_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("U1");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    [Fact]
    public void Create_WithSpecialCharacters_ShouldThrowDomainException()
    {
        var act = () => new CountryCode("U@");

        act.Should().Throw<DomainException>()
            .WithMessage("*2 uppercase letters*");
    }

    // ── Equality ───────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameCodes_ShouldBeEqual()
    {
        var a = new CountryCode("US");
        var b = new CountryCode("US");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCodes_ShouldNotBeEqual()
    {
        var a = new CountryCode("US");
        var b = new CountryCode("IN");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    // ── Implicit conversion ────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        var countryCode = new CountryCode("US");

        string result = countryCode;

        result.Should().Be("US");
    }

    [Fact]
    public void ToString_ShouldReturnCodeValue()
    {
        var countryCode = new CountryCode("IN");

        countryCode.ToString().Should().Be("IN");
    }
}
