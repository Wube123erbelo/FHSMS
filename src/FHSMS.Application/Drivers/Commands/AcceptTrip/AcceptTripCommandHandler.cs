using FHSMS.Application.Common.Interfaces;
using FHSMS.Application.Common.Exceptions;
using FHSMS.Domain.Entities;
using FHSMS.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Drivers.Commands.AcceptTrip;

public class AcceptTripCommandHandler : IRequestHandler<AcceptTripCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AcceptTripCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AcceptTripCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Must be logged in as a driver to accept a trip.");

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken)
            ?? throw new DomainException("Please complete your driver profile before accepting trips.");

        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Delivery), request.DeliveryId);

        if (delivery.DriverId == driver.Id && !delivery.DriverConfirmed)
        {
            // An admin already booked this specific driver for the trip -
            // this call is that driver saying yes, not claiming an open slot.
            delivery.ConfirmAssignment(driver.Id);
        }
        else
        {
            // Open board: first to accept wins. AssignDriver itself throws
            // DomainException if the trip was already taken or isn't Pending
            // anymore, and that message tells the driver why.
            delivery.AssignDriver(driver.Id, driver.FullName, driver.PlateNumber);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
