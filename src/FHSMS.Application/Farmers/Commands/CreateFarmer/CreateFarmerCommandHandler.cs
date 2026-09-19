using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;

namespace FHSMS.Application.Farmers.Commands.CreateFarmer;

public class CreateFarmerCommandHandler : IRequestHandler<CreateFarmerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public CreateFarmerCommandHandler(IApplicationDbContext context, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateFarmerCommand request, CancellationToken cancellationToken)
    {
        var farmer = new Farmer
        {
            Code = await _numberGenerator.NextFarmerCodeAsync(cancellationToken),
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Location = request.Location,
            BankAccountNumber = request.BankAccountNumber
        };

        _context.Farmers.Add(farmer);
        await _context.SaveChangesAsync(cancellationToken);
        return farmer.Id;
    }
}
