using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Farmers.Commands.SetFarmerActive;

public class SetFarmerActiveCommandHandler : IRequestHandler<SetFarmerActiveCommand>
{
    private readonly IApplicationDbContext _context;
    public SetFarmerActiveCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SetFarmerActiveCommand request, CancellationToken cancellationToken)
    {
        var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        farmer.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
