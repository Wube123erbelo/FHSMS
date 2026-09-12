using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Receipts.Queries.GetReceiptById;

public class GetReceiptByIdQueryHandler : IRequestHandler<GetReceiptByIdQuery, ReceiptDto>
{
    private readonly IApplicationDbContext _context;
    public GetReceiptByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ReceiptDto> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _context.Receipts.FirstOrDefaultAsync(r => r.Id == request.ReceiptId, cancellationToken)
            ?? throw new NotFoundException(nameof(Receipt), request.ReceiptId);

        return new ReceiptDto
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            PaymentId = receipt.PaymentId,
            InvoiceId = receipt.InvoiceId,
            Amount = receipt.Amount,
            Method = receipt.Method,
            TransactionReference = receipt.TransactionReference,
            IssuedAt = receipt.IssuedAt,
            IssuedBy = receipt.IssuedBy
        };
    }
}
