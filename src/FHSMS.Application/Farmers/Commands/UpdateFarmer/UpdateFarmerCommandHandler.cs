using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Farmers.Commands.UpdateFarmer;

public class UpdateFarmerCommandHandler : IRequestHandler<UpdateFarmerCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateFarmerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateFarmerCommand request, CancellationToken cancellationToken)
    {
        var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.Id == request.FarmerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Farmer), request.FarmerId);

        farmer.Name = request.Name;
        farmer.ContactPerson = request.ContactPerson;
        farmer.Phone = request.Phone;
        farmer.Location = request.Location;
        farmer.BankAccountNumber = request.BankAccountNumber;
        farmer.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
