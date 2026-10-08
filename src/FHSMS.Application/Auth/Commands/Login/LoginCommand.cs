using MediatR;

namespace FHSMS.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress = null) : IRequest<LoginResult>;

/// <summary>
/// Either a completed login (Token/RefreshToken populated) or a 2FA
/// checkpoint (RequiresTwoFactor = true, PendingUserId set, Token/RefreshToken
/// empty) - the caller must then call POST /auth/2fa/login-verify with
/// PendingUserId and a TOTP code to receive the real tokens.
/// </summary>
public record LoginResult(
    string Token,
    string RefreshToken,
    string FullName,
    string Role,
    bool RequiresTwoFactor = false,
    Guid? PendingUserId = null);
