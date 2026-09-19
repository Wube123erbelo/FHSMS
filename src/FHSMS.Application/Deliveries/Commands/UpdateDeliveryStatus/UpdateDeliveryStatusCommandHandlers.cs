using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Orders.Commands.UpdateOrderStatus;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Commands.UpdateDeliveryStatus;

public class DispatchDeliveryCommandHandler : IRequestHandler<DispatchDeliveryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;
    public DispatchDeliveryCommandHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task Handle(DispatchDeliveryCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Delivery), request.DeliveryId);

        // Same financial gate as a direct POST /orders/{id}/ship - dispatch
        // is just another door into "Shipped", and the money-before-goods
        // rule applies no matter which door was used.
        await OrderShippingGuard.EnsureInvoicePaidAsync(_context, delivery.OrderId, cancellationToken);

        delivery.Dispatch();

        // Keep the order's own state machine in step when it's following the
        // expected path (Preparing -> Shipped on dispatch). If the order was
        // never explicitly moved through Prepare(), this is a no-op rather than
        // a hard failure - dispatch shouldn't be blocked by a skipped admin step.
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == delivery.OrderId, cancellationToken);
        var justShipped = order?.Status == OrderStatus.Preparing;
        if (justShipped)
            order!.Ship();

        await _context.SaveChangesAsync(cancellationToken);

        // Same event as a direct POST /orders/{id}/ship - dispatching a
        // delivery is just another way an order reaches Shipped, and
        // inventory should deduct exactly the same way either path is used.
        if (justShipped)
            await _publisher.Publish(new FHSMS.Application.Orders.Events.OrderShippedEvent(order!.Id), cancellationToken);
    }
}

public class MarkDeliveredCommandHandler : IRequestHandler<MarkDeliveredCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkDeliveredCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Delivery), request.DeliveryId);

        // A driver can only confirm handoff on a trip they themselves
        // accepted - SuperAdmin can always mark any delivery, for
        // support/correction purposes.
        if (_currentUser.Role == "Driver")
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == _currentUser.UserId, cancellationToken);
            if (driver is null || delivery.DriverId != driver.Id)
                throw new UnauthorizedAccessException("You can only mark your own accepted trips as delivered.");
        }

        delivery.MarkDelivered(
            request.Notes, request.ReceivedByName, request.SignatureImageBase64, request.PhotoUrl, request.Latitude, request.Longitude);

        // Auto-generate the driver's payment record the instant the trip is
        // marked delivered - Delivery.TripPrice frozen here, never a
        // hand-typed amount. Same "exactly once, generated together with
        // the triggering event" guarantee as FarmerInvoice on the stock-in
        // side. Trips that were never priced (TripPrice null) simply don't
        // get a payment record - there's nothing to pay out yet.
        if (delivery.TripPrice is { } tripPrice)
        {
            _context.DriverPayments.Add(Domain.Entities.DriverPayment.Create(delivery.Id, delivery.DriverId, delivery.DriverName, tripPrice));
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == delivery.OrderId, cancellationToken);
        if (order?.Status == OrderStatus.Shipped)
            order.Deliver();

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class MarkDeliveryFailedCommandHandler : IRequestHandler<MarkDeliveryFailedCommand>
{
    private readonly IApplicationDbContext _context;
    public MarkDeliveryFailedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(MarkDeliveryFailedCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Delivery), request.DeliveryId);
        delivery.MarkFailed(request.Notes);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
