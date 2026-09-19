using FHSMS.Application.Common.Interfaces;
using FHSMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FHSMS.Application.DriverPayments.Events;

/// <summary>Texts the driver once their payment is approved - drivers are on the road, not logged into the web app, so SMS is the channel that actually reaches them. Silently skipped for payments with no linked DriverId (an ad-hoc/unregistered driver) or a driver with no phone on file.</summary>
public class NotifyDriverOnPaymentApproved : INotificationHandler<DriverPaymentApprovedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyDriverOnPaymentApproved(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Handle(DriverPaymentApprovedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.DriverId is not { } driverId)
            return;

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        if (driver is null || string.IsNullOrWhiteSpace(driver.Phone))
            return;

        var body = notification.DriverWasPaid
            ? $"Hi {driver.FullName}, your payment of {notification.Amount:N2} ETB has been approved and paid."
            : $"Hi {driver.FullName}, your payment of {notification.Amount:N2} ETB has been approved and will be processed.";

        await _notificationService.QueueAsync(
            recipientUserId: driver.UserId,
            recipientAddress: driver.Phone,
            channel: NotificationChannel.Sms,
            subject: "Payment approved",
            body: body,
            cancellationToken: cancellationToken);
    }
}
