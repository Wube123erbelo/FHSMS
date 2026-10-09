using FHSMS.Application.Auth.Commands.Login;
using MediatR;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

/// <summary>Completes a login that was paused for 2FA - see LoginCommandHandler's RequiresTwoFactor branch.</summary>
public record VerifyTwoFactorLoginCommand(Guid PendingUserId, string Code, string? IpAddress = null) : IRequest<LoginResult>;
