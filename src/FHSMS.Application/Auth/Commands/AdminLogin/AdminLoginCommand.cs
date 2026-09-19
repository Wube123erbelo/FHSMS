using FHSMS.Application.Auth.Commands.Login;
using MediatR;

namespace FHSMS.Application.Auth.Commands.AdminLogin;

/// <summary>
/// A separate login entry point for the admin portal. Credential verification
/// is identical to the regular login (same password hash check, same JWT
/// shape) - the only difference is this command refuses to issue a token for
/// any account that isn't SuperAdmin, so a hotel/farmer agent's credentials
/// simply don't work on the admin sign-in screen even though the account
/// itself is perfectly valid on the regular one.
/// </summary>
public record AdminLoginCommand(string Email, string Password, string? IpAddress = null) : IRequest<LoginResult>;
