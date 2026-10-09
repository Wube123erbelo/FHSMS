using MediatR;

namespace FHSMS.Application.Auth.Commands.Logout;

/// <summary>Revokes one refresh token (this device/session only) - the JWT itself keeps working until it naturally expires, per stateless-JWT tradeoffs.</summary>
public record LogoutCommand(string Token) : IRequest;
