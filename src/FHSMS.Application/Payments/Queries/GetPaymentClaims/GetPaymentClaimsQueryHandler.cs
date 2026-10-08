using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Payments.Queries.GetPaymentClaims;

public class GetPaymentClaimsQueryHandler : IRequestHandler<GetPaymentClaimsQuery, List<PaymentClaimDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentClaimsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PaymentClaimDto>> Handle(GetPaymentClaimsQuery request, CancellationToken cancellationToken)
    {
        var status = request.Status ?? PaymentStatus.Pending;

        var query =
            from payment in _context.Payments
            join invoice in _context.Invoices on payment.InvoiceId equals invoice.Id
            join customer in _context.Customers on invoice.CustomerId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            join bankAccount in _context.BankAccounts on payment.BankAccountId equals bankAccount.Id into bankAccounts
            from bankAccount in bankAccounts.DefaultIfEmpty()
            where payment.Status == status
            orderby payment.CreatedAt descending
            select new PaymentClaimDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = customer != null ? customer.Name : null,
                Amount = payment.Amount,
                InvoiceBalanceDue = invoice.GrandTotal - invoice.AmountPaid,
                Method = payment.Method,
                Status = payment.Status,
                Reference = payment.Reference,
                BankAccountId = payment.BankAccountId,
                BankAccountName = bankAccount != null ? bankAccount.BankName + " - " + bankAccount.AccountNumber : null,
                DeclaredBy = payment.CreatedBy,
                DeclaredAt = payment.CreatedAt
            };

        return await query.ToListAsync(cancellationToken);
    }
}
