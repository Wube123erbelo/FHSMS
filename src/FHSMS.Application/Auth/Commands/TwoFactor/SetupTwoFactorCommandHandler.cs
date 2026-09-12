using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

public class SetupTwoFactorCommandHandler : IRequestHandler<SetupTwoFactorCommand, SetupTwoFactorResult>
{
    private const string Issuer = "FHSMS";

    private readonly IApplicationDbContext _context;
    private readonly ITotpService _totp;

    public SetupTwoFactorCommandHandler(IApplicationDbContext context, ITotpService totp)
    {
        _context = context;
        _totp = totp;
    }

    public async Task<SetupTwoFactorResult> Handle(SetupTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var secret = _totp.GenerateSecret();
        user.BeginTwoFactorSetup(secret);
        await _context.SaveChangesAsync(cancellationToken);

        var uri = _totp.BuildOtpAuthUri(secret, user.Email, Issuer);
        return new SetupTwoFactorResult(secret, uri);
    }
}
