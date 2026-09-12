using FHSMS.Application.Auth.Commands.Login;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.RefreshAccessToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenGenerator refreshTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == request.Token, cancellationToken);

        // A token that's already revoked being presented again is a signal of
        // theft/replay, not an ordinary expiry - revoke every other active
        // token for that user as a precaution rather than just rejecting this one.
        if (existing is not null && existing.RevokedAt is not null)
        {
            var siblings = await _context.RefreshTokens
                .Where(r => r.UserId == existing.UserId && r.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var sibling in siblings)
                sibling.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("This refresh token has already been used. All sessions for this account have been revoked as a precaution - please sign in again.");
        }

        if (existing is null || !existing.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Account is no longer active.");

        var newRefreshToken = new RefreshToken(user.Id, _refreshTokenGenerator.Generate(), RefreshTokenLifetime, request.IpAddress);
        _context.RefreshTokens.Add(newRefreshToken);
        existing.Revoke(newRefreshToken.Id);

        var newJwt = _jwtTokenGenerator.GenerateToken(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult(newJwt, newRefreshToken.Token, user.FullName, user.Role.ToString());
    }
}
