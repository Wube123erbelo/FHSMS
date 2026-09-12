namespace FHSMS.Application.Common.Interfaces;

/// <summary>Exposes the authenticated caller's identity to handlers without depending on HttpContext.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
}
