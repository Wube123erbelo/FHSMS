using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Orders.Commands.UpdateOrderStatus;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public ConfirmOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);
        order.Confirm();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public RejectOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);
        order.Reject(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class PrepareOrderCommandHandler : IRequestHandler<PrepareOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public PrepareOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(PrepareOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);
        order.Prepare();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;
    public ShipOrderCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        await OrderShippingGuard.EnsureInvoicePaidAsync(_context, order.Id, cancellationToken);

        order.Ship();
        await _context.SaveChangesAsync(cancellationToken);
        await _publisher.Publish(new FHSMS.Application.Orders.Events.OrderShippedEvent(order.Id), cancellationToken);
    }
}

/// <summary>
/// The financial gate every path to Shipped must pass through: goods never
/// leave the warehouse - and a trip never appears for a driver to accept -
/// before the customer has actually paid in full. This is the same
/// "payment confirmed before fulfillment" rule international marketplaces
/// (Amazon, eBay, Alibaba) enforce; here it's a hard domain rule, not a
/// suggestion an admin can skip by clicking fast.
/// </summary>
internal static class OrderShippingGuard
{
    public static async Task EnsureInvoicePaidAsync(IApplicationDbContext context, Guid orderId, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);

        if (invoice is null)
            throw new Domain.Exceptions.DomainException(
                "This order has no invoice yet. Generate and collect payment on the invoice before shipping.");

        if (invoice.Status == Domain.Enums.InvoiceStatus.Cancelled)
            throw new Domain.Exceptions.DomainException("This order's invoice was cancelled. Issue a new invoice before shipping.");

        if (invoice.Status != Domain.Enums.InvoiceStatus.Paid)
            throw new Domain.Exceptions.DomainException(
                $"This order's invoice is not fully paid yet (balance due: {invoice.BalanceDue:0.00}). " +
                "Record the remaining payment before shipping - goods and drivers are never dispatched against unpaid orders.");
    }
}

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public CompleteOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // The driver marking a delivery "Delivered" is their own claim, not
        // proof - closing out the order (Completed) requires the hotel side
        // to have separately confirmed the goods actually arrived (see
        // Delivery.ConfirmReceipt / ConfirmDeliveryReceiptCommand). If there's
        // no delivery record at all for this order (e.g. hand-delivered,
        // logged outside the driver system), that's not blocked here - the
        // requirement only applies when a Delivery exists to confirm.
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == order.Id, cancellationToken);
        if (delivery is not null && !delivery.RecipientConfirmed)
            throw new Domain.Exceptions.DomainException(
                "The hotel side hasn't confirmed receipt of this delivery yet. Confirm delivery before completing the order.");

        order.Complete();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public ReturnOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ReturnOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);
        order.Return(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
