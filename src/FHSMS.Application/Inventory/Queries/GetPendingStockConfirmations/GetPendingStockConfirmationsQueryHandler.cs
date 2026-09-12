using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Queries.GetPendingStockConfirmations;

public class GetPendingStockConfirmationsQueryHandler : IRequestHandler<GetPendingStockConfirmationsQuery, List<InventoryTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    public GetPendingStockConfirmationsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<InventoryTransactionDto>> Handle(GetPendingStockConfirmationsQuery request, CancellationToken cancellationToken)
    {
        var pending = await _context.InventoryTransactions
            .Where(t => t.Type == InventoryTransactionType.Receiving && !t.IsConfirmed)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var productIds = pending.Select(t => t.ProductId).Distinct().ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);

        var farmerIds = pending.Where(t => t.FarmerId != null).Select(t => t.FarmerId!.Value).Distinct().ToList();
        var farmers = await _context.Farmers.Where(f => farmerIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, cancellationToken);

        var agentIds = pending.Where(t => t.AgentUserId != null).Select(t => t.AgentUserId!.Value).Distinct().ToList();
        var agents = await _context.Users.Where(u => agentIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, cancellationToken);

        var transactionIds = pending.Select(t => t.Id).ToList();
        var farmerInvoices = await _context.FarmerInvoices
            .Where(f => transactionIds.Contains(f.InventoryTransactionId))
            .ToDictionaryAsync(f => f.InventoryTransactionId, cancellationToken);

        return pending.Select(t =>
        {
            farmerInvoices.TryGetValue(t.Id, out var invoice);
            return new InventoryTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = products.TryGetValue(t.ProductId, out var p) ? p.Name : null,
                Type = t.Type,
                QuantityChange = t.QuantityChange,
                Reference = t.Reference,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                FarmerId = t.FarmerId,
                FarmerName = t.FarmerId != null && farmers.TryGetValue(t.FarmerId.Value, out var f) ? f.Name : null,
                AgentUserId = t.AgentUserId,
                AgentName = t.AgentUserId != null && agents.TryGetValue(t.AgentUserId.Value, out var a) ? a.FullName : null,
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
