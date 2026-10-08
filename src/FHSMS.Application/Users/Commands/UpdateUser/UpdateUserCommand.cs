using FHSMS.Domain.Enums;
using MediatR;

namespace FHSMS.Application.Users.Commands.UpdateUser;

/// <summary>Admin-only: change a user's role, active status, or contact info. Never touches the password.</summary>
public record UpdateUserCommand(Guid UserId, string FullName, UserRole Role, bool IsActive, string? Phone = null, string? Location = null) : IRequest;
