using FHSMS.Application.Auth.Commands.Login;
using MediatR;

namespace FHSMS.Application.Auth.Commands.RefreshAccessToken;

/// <summary>
/// Exchanges a still-valid refresh token for a new JWT + new refresh token,
/// and revokes the one just used (rotation - see RefreshToken.Revoke remarks).
/// </summary>
public record RefreshTokenCommand(string Token, string? IpAddress = null) : IRequest<LoginResult>;
