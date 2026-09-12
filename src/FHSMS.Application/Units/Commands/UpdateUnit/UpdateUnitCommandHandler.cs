using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Units.Commands.UpdateUnit;

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Unit), request.UnitId);

        unit.Name = request.Name;
        unit.Abbreviation = request.Abbreviation;
        unit.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
