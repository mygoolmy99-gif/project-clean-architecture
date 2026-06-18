namespace HRMS.Domain.Common;

/// <summary>
/// Exception thrown when a domain invariant is violated.
/// Handled by the global exception middleware at the API boundary.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
