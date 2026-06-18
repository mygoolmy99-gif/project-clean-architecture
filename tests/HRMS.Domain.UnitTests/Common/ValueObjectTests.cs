using FluentAssertions;
using HRMS.Domain.Common;

namespace HRMS.Domain.UnitTests.Common;

public class ValueObjectTests
{
    // ── Test helper: concrete ValueObject for testing ──────────────────────

    private sealed class TestValueObject(string component1, int component2) : ValueObject
    {
        public string Component1 { get; } = component1;
        public int Component2 { get; } = component2;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Component1;
            yield return Component2;
        }
    }

    private sealed class OtherValueObject(string value) : ValueObject
    {
        public string Value { get; } = value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    // ── Equals ─────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("test", 42);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentComponents_ShouldReturnFalse()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("other", 99);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        var a = new TestValueObject("test", 42);

        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentDerivedType_ShouldReturnFalse()
    {
        var a = new TestValueObject("test", 42);
        var b = new OtherValueObject("test");

        a.Equals(b).Should().BeFalse();
    }

    // ── GetHashCode ────────────────────────────────────────────────────────

    [Fact]
    public void GetHashCode_SameComponents_ShouldReturnSameHash()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("test", 42);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentComponents_ShouldReturnDifferentHash()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("other", 99);

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    // ── Operators ──────────────────────────────────────────────────────────

    [Fact]
    public void OperatorEquals_SameComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("test", 42);

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_DifferentComponents_ShouldReturnFalse()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("other", 99);

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_DifferentComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("test", 42);
        var b = new TestValueObject("other", 99);

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_BothNull_ShouldReturnTrue()
    {
        TestValueObject? a = null;
        TestValueObject? b = null;

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_LeftNull_ShouldReturnFalse()
    {
        TestValueObject? a = null;
        var b = new TestValueObject("test", 42);

        (a == b).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_RightNull_ShouldReturnFalse()
    {
        var a = new TestValueObject("test", 42);
        TestValueObject? b = null;

        (a == b).Should().BeFalse();
    }
}
