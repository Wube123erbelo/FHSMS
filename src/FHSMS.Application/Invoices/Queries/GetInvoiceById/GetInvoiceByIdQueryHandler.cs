using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invoice), request.InvoiceId);

        // Same rule as GetInvoicesQueryHandler applied to the single-invoice
        // lookup: a hotel agent can only open an invoice for an order they
        // themselves placed, never one belonging to a colleague.
        if (_currentUser.Role == "HotelAgent")
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == invoice.OrderId, cancellationToken);
            if (order?.AgentUserId != _currentUser.UserId)
                throw new NotFoundException(nameof(Invoice), request.InvoiceId);
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            OrderId = invoice.OrderId,
            CustomerId = invoice.CustomerId,
            InvoiceDate = invoice.InvoiceDate,
            Status = invoice.Status,
            TaxWasEnabled = invoice.TaxWasEnabled,
            TaxMode = invoice.TaxMode,
            Subtotal = invoice.Subtotal,
            TaxableAmount = invoice.TaxableAmount,
            TaxAmount = invoice.TaxAmount,
            Discount = invoice.Discount,
            PlatformCommissionRateApplied = invoice.PlatformCommissionRateApplied,
            PlatformCommissionAmount = invoice.PlatformCommissionAmount,
            HotelAgentBonusAmount = invoice.HotelAgentBonusAmount,
            GrandTotal = invoice.GrandTotal,
            AmountPaid = invoice.AmountPaid,
            BalanceDue = invoice.BalanceDue,
            Items = invoice.Items.Select(i => new InvoiceItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineSubtotal = i.LineSubtotal,
                TaxProfileApplied = i.TaxProfileApplied,
                TaxRateApplied = i.TaxRateApplied,
                TaxableAmount = i.TaxableAmount,
                TaxAmount = i.TaxAmount,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}
