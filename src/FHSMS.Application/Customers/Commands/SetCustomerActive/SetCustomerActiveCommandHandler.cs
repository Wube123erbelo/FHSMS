using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Customers.Commands.SetCustomerActive;

public class SetCustomerActiveCommandHandler : IRequestHandler<SetCustomerActiveCommand>
{
    private readonly IApplicationDbContext _context;
    public SetCustomerActiveCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(SetCustomerActiveCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        customer.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
