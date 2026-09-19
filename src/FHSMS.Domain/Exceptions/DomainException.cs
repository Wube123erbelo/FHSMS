namespace FHSMS.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule enforced by the domain layer is violated.
/// This is distinct from validation errors (which belong to the Application layer) -
/// a DomainException means "this state can never be valid", not "this input was malformed".
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
