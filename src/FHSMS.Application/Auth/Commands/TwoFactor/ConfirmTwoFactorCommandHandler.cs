using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.TwoFactor;

public class ConfirmTwoFactorCommandHandler : IRequestHandler<ConfirmTwoFactorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITotpService _totp;

    public ConfirmTwoFactorCommandHandler(IApplicationDbContext context, ITotpService totp)
    {
        _context = context;
        _totp = totp;
    }

    public async Task Handle(ConfirmTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (user.TwoFactorSecret is null || !_totp.ValidateCode(user.TwoFactorSecret, request.Code))
            throw new UnauthorizedAccessException("That code is incorrect or has expired. Please try again.");

        user.ConfirmTwoFactor();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
