using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateUserCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.FullName = request.FullName;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.Phone = request.Phone;
        user.Location = request.Location;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
