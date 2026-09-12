using MediatR;

namespace FHSMS.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Issues a password reset token and queues a notification containing it.
/// Deliberately always succeeds from the caller's point of view (no "email not
/// found" leak) - if the email doesn't match an active user, nothing happens
/// but the response looks identical either way.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest;
