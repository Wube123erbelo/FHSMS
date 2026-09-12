using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace FHSMS.Application.Deliveries.Commands.CreateDelivery;

public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDeliveryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
    {
        // A clear 404 with a real message here, rather than silently creating
        // an orphaned delivery for an order that doesn't exist - this is the
        // fix for deliveries created against a stale/mistyped order id ever
        // surfacing as an opaque "request failed with status code 404" with
        // nothing to explain it.
        var orderExists = await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken);
        if (!orderExists)
            throw new NotFoundException(nameof(Order), request.OrderId);

        var delivery = new Delivery(request.OrderId, request.DestinationAddress, null, null, request.OriginLocation, request.TripPrice);

        if (request.DriverId is { } driverId)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken)
                ?? throw new NotFoundException(nameof(Driver), driverId);

            delivery.AssignDriverByAdmin(driver.Id, driver.FullName, driver.PlateNumber);
        }

        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);
        return delivery.Id;
    }
}
