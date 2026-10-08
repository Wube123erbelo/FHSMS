using MediatR;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

/// <summary>Begins 2FA setup for the authenticated user - generates a secret, returns it plus the otpauth:// URI for a QR code. Not yet enabled until ConfirmTwoFactorCommand succeeds.</summary>
public record SetupTwoFactorCommand(Guid UserId) : IRequest<SetupTwoFactorResult>;

public record SetupTwoFactorResult(string Secret, string OtpAuthUri);
