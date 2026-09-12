using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Queries.GetDeliveryByOrder;

public class GetDeliveryByOrderQueryHandler : IRequestHandler<GetDeliveryByOrderQuery, DeliveryDto?>
{
    private readonly IApplicationDbContext _context;
    public GetDeliveryByOrderQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DeliveryDto?> Handle(GetDeliveryByOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return await _context.Deliveries
            .Where(d => d.OrderId == request.OrderId)
            .Select(d => new DeliveryDto
            {
                Id = d.Id,
                OrderId = d.OrderId,
                OrderNumber = order != null ? order.OrderNumber : null,
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
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
