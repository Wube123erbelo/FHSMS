using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Invoices.Commands.CancelInvoice;

public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand>
{
    private readonly IApplicationDbContext _context;
    public CancelInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        invoice.Cancel();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
