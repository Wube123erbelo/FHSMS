using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.Deliveries.Events;

/// <summary>Texts the driver (and puts it in their in-app list, via their own UserId) that a trip has been assigned to them and is waiting on their confirmation - see Delivery.ConfirmAssignment.</summary>
public class NotifyDriverOnDeliveryAssigned : INotificationHandler<DeliveryAssignedByAdminEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyDriverOnDeliveryAssigned(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(DeliveryAssignedByAdminEvent notification, CancellationToken cancellationToken)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == notification.DriverId, cancellationToken);
        if (driver is null || string.IsNullOrWhiteSpace(driver.Phone))
            return;

        var body = notification.TripPrice is { } price
            ? $"Hi {driver.FullName}, you've been assigned a trip to {notification.DestinationAddress} paying {price:N2} ETB. Confirm it in the Driver Portal."
            : $"Hi {driver.FullName}, you've been assigned a trip to {notification.DestinationAddress}. Confirm it in the Driver Portal.";

        await _notificationService.QueueAsync(
            recipientUserId: driver.UserId,
            recipientAddress: driver.Phone,
            channel: NotificationChannel.Sms,
            subject: "New trip assigned",
            body: body,
            cancellationToken: cancellationToken);
    }
}
