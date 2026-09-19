using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Queries.GetStockHistory;

public class GetStockHistoryQueryHandler : IRequestHandler<GetStockHistoryQuery, List<InventoryTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryTransactionDto>> Handle(GetStockHistoryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _context.InventoryTransactions
            .Where(t => t.ProductId == request.ProductId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var transactionIds = transactions.Select(t => t.Id).ToList();
        var farmerInvoices = await _context.FarmerInvoices
            .Where(f => transactionIds.Contains(f.InventoryTransactionId))
            .ToDictionaryAsync(f => f.InventoryTransactionId, cancellationToken);

        return transactions.Select(t =>
        {
            farmerInvoices.TryGetValue(t.Id, out var invoice);
            return new InventoryTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                Type = t.Type,
                QuantityChange = t.QuantityChange,
                Reference = t.Reference,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                FarmerId = t.FarmerId,
                AgentUserId = t.AgentUserId,
                IsConfirmed = t.IsConfirmed,
                ConfirmedAt = t.ConfirmedAt,
                FarmerPaymentConfirmed = t.FarmerPaymentConfirmed,
                AmountPaidToFarmer = t.AmountPaidToFarmer,
                FarmerInvoiceId = invoice?.Id,
                FarmerInvoiceNumber = invoice?.InvoiceNumber,
                FarmerInvoiceStatus = invoice?.Status,
                FarmerInvoiceTotalAmount = invoice?.TotalAmount
            };
        }).ToList();
    }
}
