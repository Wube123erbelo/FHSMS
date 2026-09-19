using FHSMS.Application.Auth.Commands.Login;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.AdminLogin;

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, LoginResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public AdminLoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResult> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Role != UserRole.SuperAdmin)
            throw new UnauthorizedAccessException("This account does not have admin access. Use the staff/customer sign-in instead.");

        if (user.TwoFactorEnabled)
        {
            return new LoginResult(string.Empty, string.Empty, user.FullName, user.Role.ToString(), RequiresTwoFactor: true, PendingUserId: user.Id);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        var refreshToken = new RefreshToken(user.Id, _refreshTokenGenerator.Generate(), RefreshTokenLifetime, request.IpAddress);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResult(token, refreshToken.Token, user.FullName, user.Role.ToString());
    }
}
