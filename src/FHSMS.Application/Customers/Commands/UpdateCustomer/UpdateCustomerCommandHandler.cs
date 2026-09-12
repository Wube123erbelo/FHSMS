using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Customers.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateCustomerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        customer.Name = request.Name;
        customer.ContactPerson = request.ContactPerson;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Address = request.Address;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
