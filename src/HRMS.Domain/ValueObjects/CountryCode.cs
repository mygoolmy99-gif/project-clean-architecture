using System.Text.RegularExpressions;
using HRMS.Domain.Common;

namespace HRMS.Domain.ValueObjects;

/// <summary>
/// ISO 3166-1 alpha-2 country code value object.
/// Validates exactly 2 uppercase ASCII letters (e.g., "US", "IN", "GB").
/// </summary>
public sealed partial class CountryCode : ValueObject
{
    public string Value { get; }

    public CountryCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Country code cannot be empty.");

        if (!IsoAlpha2Pattern().IsMatch(value))
            throw new DomainException(
                $"Country code '{value}' must be exactly 2 uppercase letters (ISO 3166-1 alpha-2).");

        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(CountryCode code) => code.Value;

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex IsoAlpha2Pattern();
}
