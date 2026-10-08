using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Queries.GetDeliveries;

public class GetDeliveriesQueryHandler : IRequestHandler<GetDeliveriesQuery, List<DeliveryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetDeliveriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<DeliveryDto>> Handle(GetDeliveriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Deliveries.AsQueryable();
        if (request.Status is { } status)
            query = query.Where(d => d.Status == status);

        var deliveries = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);
        var orderIds = deliveries.Select(d => d.OrderId).ToList();
        var orders = await _context.Orders
            .Where(o => orderIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, cancellationToken);

        return deliveries.Select(d => new DeliveryDto
        {
            Id = d.Id,
            OrderId = d.OrderId,
            OrderNumber = orders.TryGetValue(d.OrderId, out var order) ? order.OrderNumber : "-",
            DestinationAddress = d.DestinationAddress,
            OriginLocation = d.OriginLocation,
            TripPrice = d.TripPrice,
            DriverId = d.DriverId,
            DriverName = d.DriverName,
            VehicleInfo = d.VehicleInfo,
            DriverConfirmed = d.DriverConfirmed,
            RecipientConfirmed = d.RecipientConfirmed,
            RecipientConfirmedAt = d.RecipientConfirmedAt,
            Status = d.Status,
            DispatchedAt = d.DispatchedAt,
            DeliveredAt = d.DeliveredAt,
            ReceivedByName = d.ReceivedByName,
            HasSignature = d.SignatureImageBase64 != null,
            PhotoUrl = d.PhotoUrl,
            DeliveryLatitude = d.DeliveryLatitude,
            DeliveryLongitude = d.DeliveryLongitude
        }).ToList();
    }
}
