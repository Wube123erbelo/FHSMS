using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Users.Commands.SetUserActive;

public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand>
{
    private readonly IApplicationDbContext _context;
    public SetUserActiveCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
