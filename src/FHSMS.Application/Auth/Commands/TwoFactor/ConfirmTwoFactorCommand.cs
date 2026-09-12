using MediatR;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

/// <summary>Completes 2FA setup - the user must prove they scanned the QR correctly by submitting one valid code.</summary>
public record ConfirmTwoFactorCommand(Guid UserId, string Code) : IRequest;
