using MediatR;

namespace FHSMS.Application.Auth.Commands.ChangePassword;

/// <summary>Self-service password change for the currently authenticated user.</summary>
public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest;
