using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Queries.GetAvailableTrips;

public class GetAvailableTripsQueryHandler : IRequestHandler<GetAvailableTripsQuery, List<TripDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAvailableTripsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TripDto>> Handle(GetAvailableTripsQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Driver? myDriver = null;
        if (_currentUser.UserId is { } userId)
            myDriver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);

        // Two kinds of trip belong on this board: still-open trips nobody has
        // claimed yet, and trips an admin booked for THIS driver specifically
        // that they haven't confirmed yet - without the second case, an
        // admin-assigned trip would be invisible to the driver and could
        // never be confirmed.
        var deliveries = await _context.Deliveries
            .Where(d => d.Status == DeliveryStatus.Pending &&
                (d.DriverId == null || (myDriver != null && d.DriverId == myDriver.Id && !d.DriverConfirmed)))
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        // Money-before-goods: an unclaimed trip only appears once its order's
        // invoice is fully paid - same rule enforced again (belt and
        // suspenders) at actual Dispatch time via OrderShippingGuard. A trip
        // an admin already booked for this driver is exempt from that filter
        // here (payment was already required for the admin to legally ship it
        // via CreateDelivery's own flow at the point it's actually dispatched) -
        // but showing it lets the driver see and confirm ahead of time.
        var openOrderIds = deliveries.Where(d => d.DriverId == null).Select(d => d.OrderId).ToList();
        var paidOrderIds = await _context.Invoices
            .Where(i => openOrderIds.Contains(i.OrderId) && i.Status == Domain.Enums.InvoiceStatus.Paid)
            .Select(i => i.OrderId)
            .ToListAsync(cancellationToken);

        var visibleDeliveries = deliveries
            .Where(d => d.DriverId != null || paidOrderIds.Contains(d.OrderId))
            .ToList();

        return await MapToTripsAsync(visibleDeliveries, cancellationToken);
    }

    internal static async Task<List<TripDto>> MapToTripsAsync(
        List<Domain.Entities.Delivery> deliveries, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var orderIds = deliveries.Select(d => d.OrderId).ToList();
        var orders = await context.Orders
            .Include(o => o.Items)
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync(cancellationToken);
        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = await context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var result = new List<TripDto>();
        foreach (var delivery in deliveries)
        {
            var order = orders.FirstOrDefault(o => o.Id == delivery.OrderId);
            if (order is null) continue;

            var items = order.Items.ToList();
            var totalQuantity = items.Sum(i => i.Quantity);
            var summary = items.Count switch
            {
                0 => "-",
                1 => $"{items[0].ProductName} {items[0].Quantity:0.##}",
                _ => $"{items[0].ProductName} + {items.Count - 1} more"
            };

            customers.TryGetValue(order.CustomerId, out var customer);

            result.Add(new TripDto
            {
                DeliveryId = delivery.Id,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OriginLocation = delivery.OriginLocation,
                DestinationAddress = delivery.DestinationAddress,
                DestinationName = customer?.Name ?? "-",
                ProductSummary = summary,
                TotalQuantity = totalQuantity,
                TripPrice = delivery.TripPrice,
                OrderDate = order.OrderDate,
                Status = delivery.Status.ToString(),
                DriverId = delivery.DriverId,
                DriverConfirmed = delivery.DriverConfirmed
            });
        }
        return result;
    }

    private Task<List<TripDto>> MapToTripsAsync(List<Domain.Entities.Delivery> deliveries, CancellationToken cancellationToken)
        => MapToTripsAsync(deliveries, _context, cancellationToken);
}
