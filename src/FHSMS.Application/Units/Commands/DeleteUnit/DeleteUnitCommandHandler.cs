using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Units.Commands.DeleteUnit;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        unit.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
