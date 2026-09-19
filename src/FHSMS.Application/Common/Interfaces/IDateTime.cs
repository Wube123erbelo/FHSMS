namespace FHSMS.Application.Common.Interfaces;

/// <summary>Testable wrapper around the system clock.</summary>
public interface IDateTime
{
    DateTime UtcNow { get; }
}
