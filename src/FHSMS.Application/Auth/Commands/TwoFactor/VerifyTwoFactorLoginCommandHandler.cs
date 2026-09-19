using FHSMS.Application.Auth.Commands.Login;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

public class VerifyTwoFactorLoginCommandHandler : IRequestHandler<VerifyTwoFactorLoginCommand, LoginResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IApplicationDbContext _context;
    private readonly ITotpService _totp;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public VerifyTwoFactorLoginCommandHandler(
        IApplicationDbContext context, ITotpService totp, IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenGenerator refreshTokenGenerator)
    {
        _context = context;
        _totp = totp;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResult> Handle(VerifyTwoFactorLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.PendingUserId, cancellationToken);

        if (user is null || !user.IsActive || !user.TwoFactorEnabled || user.TwoFactorSecret is null)
            throw new UnauthorizedAccessException("Invalid two-factor login attempt.");

        if (!_totp.ValidateCode(user.TwoFactorSecret, request.Code))
            throw new UnauthorizedAccessException("That code is incorrect or has expired.");

        var token = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = new RefreshToken(user.Id, _refreshTokenGenerator.Generate(), RefreshTokenLifetime, request.IpAddress);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult(token, refreshToken.Token, user.FullName, user.Role.ToString());
    }
}
