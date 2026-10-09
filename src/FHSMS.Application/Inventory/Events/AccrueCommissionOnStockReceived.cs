using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Inventory.Events;

/// <summary>
/// Accrues a commission for the farmer agent who logged a stock receipt, if
/// any - the flat-rate-per-quantity counterpart to
/// AccrueCommissionOnInvoiceIssued. E.g. with the default seeded rule
/// (FarmerAgent, FlatRatePerQuantity, 0.50 birr/kg), receiving 40 kg of
/// tomatoes accrues 20.00 birr for the agent who logged it.
/// </summary>
public class AccrueCommissionOnStockReceived : INotificationHandler<StockReceivedEvent>
{
    private readonly IApplicationDbContext _context;

    public AccrueCommissionOnStockReceived(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StockReceivedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.AgentUserId is not { } agentUserId)
            return; // logged directly (e.g. by an admin) with no agent attached - nothing to accrue

        var rule = await _context.CommissionRules
            .FirstOrDefaultAsync(r => r.AgentType == AgentType.FarmerAgent && r.IsActive, cancellationToken);

        // Opt-in configuration, same as tax and the invoice side. Also skip if
        // FarmerAgent has been reconfigured onto a percentage-of-invoice basis -
        // a stock receipt has no invoice/grand-total to take a percentage of.
        if (rule is null || rule.Basis != CommissionBasis.FlatRatePerQuantity)
            return;

        // Farmers supply produce in different units (kg, quintal, litre, ...) -
        // resolve whichever rate applies to this product's actual unit
        // (per-unit override if the admin set one, otherwise the rule's
        // default), not a single hard-coded per-kg figure.
        var product = await _context.Products
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == notification.ProductId, cancellationToken);

        var effectiveRate = product is not null ? rule.GetEffectiveFlatRate(product.UnitId) : rule.FlatRateAmount;
        if (effectiveRate is not { } flatRate)
            return;

        var commission = Domain.Entities.Commission.ForStockReceipt(
            agentUserId, notification.InventoryTransactionId, notification.Quantity, flatRate);

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
