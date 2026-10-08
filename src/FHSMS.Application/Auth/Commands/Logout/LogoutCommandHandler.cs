using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _context;
    public LogoutCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.Token, cancellationToken);
        if (token is not null && token.RevokedAt is null)
        {
            token.Revoke();
            await _context.SaveChangesAsync(cancellationToken);
        }
        // Silently no-op if the token doesn't exist or is already revoked - logout is idempotent.
    }
}
