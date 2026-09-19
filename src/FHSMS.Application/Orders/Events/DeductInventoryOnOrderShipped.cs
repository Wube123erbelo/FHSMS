using FHSMS.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Events;

/// <summary>
/// Automatically reduces stock on hand for every line item when an order
/// ships - this is the answer to "how does inventory reduce automatically
/// when an order is placed": it deliberately does NOT happen at order
/// creation (an order can still be rejected/cancelled before it ever leaves
/// the warehouse), and does NOT happen at Confirm/Prepare either - stock
/// only actually leaves the building at Ship, so that's when the ledger
/// records it. One Issuing InventoryTransaction per order line, tagged with
/// the OrderId so GET /api/inventory/products/{id}/history shows exactly
/// which order each deduction came from.
/// </summary>
public class DeductInventoryOnOrderShipped : INotificationHandler<OrderShippedEvent>
{
    private readonly IApplicationDbContext _context;

    public DeductInventoryOnOrderShipped(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);
        if (order is null) return;

        foreach (var item in order.Items)
        {
            var transaction = Domain.Entities.InventoryTransaction.Issue(
                item.ProductId, item.Quantity, order.Id, $"Order {order.OrderNumber} shipped");
            _context.InventoryTransactions.Add(transaction);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
