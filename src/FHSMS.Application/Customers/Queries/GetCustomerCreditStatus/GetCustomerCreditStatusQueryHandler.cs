using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Customers.Queries.GetCustomerCreditStatus;

public class GetCustomerCreditStatusQueryHandler : IRequestHandler<GetCustomerCreditStatusQuery, CustomerCreditStatusDto>
{
    private readonly IApplicationDbContext _context;
    public GetCustomerCreditStatusQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<CustomerCreditStatusDto> Handle(GetCustomerCreditStatusQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var outstandingInvoices = await _context.Invoices
            .Where(i => i.CustomerId == request.CustomerId
                && i.Status != InvoiceStatus.Cancelled
                && i.Status != InvoiceStatus.Paid)
            .ToListAsync(cancellationToken);

        var exposure = outstandingInvoices.Sum(i => i.BalanceDue);

        return new CustomerCreditStatusDto
        {
            CustomerId = customer.Id,
            CreditLimit = customer.CreditLimit,
            CurrentExposure = exposure,
            AvailableCredit = customer.CreditLimit.HasValue ? customer.CreditLimit.Value - exposure : null,
            OverLimit = customer.CreditLimit.HasValue && exposure > customer.CreditLimit.Value,
            OutstandingInvoiceCount = outstandingInvoices.Count
        };
    }
}
