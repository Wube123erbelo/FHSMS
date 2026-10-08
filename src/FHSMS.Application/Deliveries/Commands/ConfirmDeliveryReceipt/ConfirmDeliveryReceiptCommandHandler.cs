using FHSMS.Application.Common.Exceptions;
using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Commands.ConfirmDeliveryReceipt;

public class ConfirmDeliveryReceiptCommandHandler : IRequestHandler<ConfirmDeliveryReceiptCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ConfirmDeliveryReceiptCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ConfirmDeliveryReceiptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Must be logged in to confirm a delivery.");

        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Delivery), request.DeliveryId);

        // A hotel agent may only confirm deliveries for orders they
        // themselves placed - an admin can confirm any of them (support/
        // fallback, e.g. when the order came from the hotel-customer portal
        // directly rather than through an agent).
        if (_currentUser.Role == "HotelAgent")
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == delivery.OrderId, cancellationToken);
            if (order?.AgentUserId != userId)
                throw new UnauthorizedAccessException("You can only confirm delivery of orders you placed yourself.");
        }

        delivery.ConfirmReceipt(userId);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
