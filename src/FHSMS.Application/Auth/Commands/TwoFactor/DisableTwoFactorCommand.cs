using MediatR;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

/// <summary>Disables 2FA - requires the current password as proof this is really the account owner, not a stolen session.</summary>
public record DisableTwoFactorCommand(Guid UserId, string CurrentPassword) : IRequest;
