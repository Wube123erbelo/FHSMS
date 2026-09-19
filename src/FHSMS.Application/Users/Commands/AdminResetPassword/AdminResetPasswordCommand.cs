using MediatR;

namespace FHSMS.Application.Users.Commands.AdminResetPassword;

/// <summary>
/// Lets a SuperAdmin set a new password for any user directly - no email
/// required. This is how a field agent (student) who forgot their password,
/// or hasn't received it yet, gets working credentials: the admin issues one
/// in person/by phone, same as at account creation. The self-service
/// forgot-password email flow still exists separately for anyone who does
/// have working email/SMS.
/// </summary>
public record AdminResetPasswordCommand(Guid UserId, string NewPassword) : IRequest;
