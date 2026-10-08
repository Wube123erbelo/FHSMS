using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;

namespace FHSMS.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberGenerator _numberGenerator;

    public CreateCustomerCommandHandler(IApplicationDbContext context, IDocumentNumberGenerator numberGenerator)
    {
        _context = context;
        _numberGenerator = numberGenerator;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Code = await _numberGenerator.NextCustomerCodeAsync(cancellationToken),
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            CreditLimit = request.CreditLimit
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }
}
